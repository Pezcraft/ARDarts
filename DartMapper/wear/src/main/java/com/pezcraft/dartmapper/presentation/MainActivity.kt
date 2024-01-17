package com.pezcraft.dartmapper.presentation

import android.content.Context
import android.hardware.Sensor
import android.hardware.SensorEvent
import android.hardware.SensorEventListener
import android.hardware.SensorManager
import android.opengl.Matrix
import android.os.Bundle
import android.util.Log
import android.view.WindowManager
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.tooling.preview.Devices
import androidx.compose.ui.tooling.preview.Preview
import androidx.core.splashscreen.SplashScreen.Companion.installSplashScreen
import androidx.lifecycle.lifecycleScope
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.wear.compose.foundation.lazy.AutoCenteringParams
import androidx.wear.compose.foundation.lazy.ScalingLazyColumn
import androidx.wear.compose.material.MaterialTheme
import androidx.wear.compose.material.PositionIndicator
import androidx.wear.compose.material.Scaffold
import androidx.wear.compose.material.TimeText
import androidx.wear.compose.material.Vignette
import androidx.wear.compose.material.VignettePosition
import com.google.android.horologist.annotations.ExperimentalHorologistApi
import com.google.android.horologist.compose.layout.rememberColumnState
import com.google.android.horologist.compose.rotaryinput.rotaryWithScroll
import com.pezcraft.dartmapper.R
import com.pezcraft.dartmapper.components.IpTextField
import com.pezcraft.dartmapper.domain.MainViewModel
import com.pezcraft.dartmapper.domain.UdpClient
import com.pezcraft.dartmapper.presentation.theme.DartMapperTheme
import com.pezcraft.dartmapper.util.DataStoreManager
import kotlinx.coroutines.launch
import kotlin.math.sqrt

class MainActivity : ComponentActivity(), SensorEventListener {

    private lateinit var sensorManager: SensorManager
    private var accelerometer: Sensor? = null
    private var gravity: Sensor? = null
    private var rotationVector: Sensor? = null
    private var gravityValues: FloatArray? = null

    private val udpClient = UdpClient()

    private val rotationVectorDataRaw = mutableStateOf(floatArrayOf())
    private val orientationDataRaw = mutableStateOf(floatArrayOf())

    override fun onCreate(savedInstanceState: Bundle?) {
        installSplashScreen()

        super.onCreate(savedInstanceState)

        setTheme(android.R.style.Theme_DeviceDefault)

        sensorManager = getSystemService(Context.SENSOR_SERVICE) as SensorManager
        accelerometer = sensorManager.getDefaultSensor(Sensor.TYPE_ACCELEROMETER)
        gravity = sensorManager.getDefaultSensor(Sensor.TYPE_GRAVITY)
        rotationVector = sensorManager.getDefaultSensor(Sensor.TYPE_ROTATION_VECTOR)

        accelerometer?.let {
            sensorManager.registerListener(this, it, SensorManager.SENSOR_DELAY_FASTEST)
        }

        gravity?.let {
            sensorManager.registerListener(this, it, SensorManager.SENSOR_DELAY_FASTEST)
        }

        rotationVector?.let {
            sensorManager.registerListener(this, it, SensorManager.SENSOR_DELAY_FASTEST)
        }

        window.addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON)

        setContent {
            WearApp()
        }
    }

    override fun onDestroy() {
        super.onDestroy()
        sensorManager.unregisterListener(this)
    }

    override fun onSensorChanged(event: SensorEvent?) {
        when (event?.sensor?.type) {
            Sensor.TYPE_ROTATION_VECTOR -> {
                val rotationMatrix = FloatArray(9)
                SensorManager.getRotationMatrixFromVector(rotationMatrix, event.values)
                rotationVectorDataRaw.value = rotationMatrix

                val orientationValues = FloatArray(3)
                SensorManager.getOrientation(rotationMatrix, orientationValues)
                orientationDataRaw.value = orientationValues
                val orientation = orientationValues.toDegrees()

                sendMessageToUnity("ORIENTATION|${
                    floatArrayOf(
                        orientation[1],
                        orientation[0],
                        orientation[2]
                    ).floatArrayToString()
                }")
                Log.d(
                    "Orientation",
                    "Values: (" + orientation[0] + ", " + orientation[1] + ", " + orientation[2] + ")"
                )
            }

            Sensor.TYPE_ACCELEROMETER -> {
                if (rotationVectorDataRaw.value.isNotEmpty()) {
                    val alpha = 0.8f

                    val gravity = gravityValues!!

                    gravity[0] = alpha * gravity[0] + (1 - alpha) * event.values[0]
                    gravity[1] = alpha * gravity[1] + (1 - alpha) * event.values[1]
                    gravity[2] = alpha * gravity[2] + (1 - alpha) * event.values[2]

                    val x = event.values[0] - gravity[0]
                    val y = event.values[1] - gravity[1]
                    val z = event.values[2] - gravity[2]

                    val deviceRelativeAcceleration = FloatArray(4)
                    deviceRelativeAcceleration[0] = x
                    deviceRelativeAcceleration[1] = y
                    deviceRelativeAcceleration[2] = z
                    deviceRelativeAcceleration[3] = 0f

                    // Change the device relative acceleration values to earth relative values
                    // X axis -> East
                    // Y axis -> North Pole
                    // Z axis -> Sky
                    val rotationMatrix = FloatArray(16)
                    val earthAcc = FloatArray(16)

                    SensorManager.getRotationMatrixFromVector(rotationMatrix, rotationVectorDataRaw.value)

                    val inv = FloatArray(16)

                    Matrix.invertM(inv, 0, rotationMatrix, 0)
                    Matrix.multiplyMV(earthAcc, 0, inv, 0, deviceRelativeAcceleration, 0)

                    detectThrow(earthAcc)
                }
            }

            Sensor.TYPE_GRAVITY -> {
                gravityValues = event.values
            }
        }
    }

    override fun onAccuracyChanged(sensor: Sensor?, accuracy: Int) { }

    private fun detectThrow(currentAccelerometerData: FloatArray) {
        val x = currentAccelerometerData[0]
        val y = currentAccelerometerData[1]
        val z = currentAccelerometerData[2]

        val acceleration = sqrt((x * x + y * y + z * z).toDouble())
        if (acceleration > 25) {
            sendMessageToUnity("THROW|${
                floatArrayOf(
                    y,
                    x+90,
                    acceleration.toFloat(),
                ).floatArrayToString()
            }")
        }
    }

    private fun sendMessageToUnity(message: String) {
        lifecycleScope.launch {
            val ip = DataStoreManager.getIp(this@MainActivity)
            udpClient.sendMessage(ip, message)
        }
    }

    private fun FloatArray.toDegrees(): FloatArray {
        val azimuth: Float = this[0] * 180f / Math.PI.toFloat()
        val pitch: Float = this[1] * 180f / Math.PI.toFloat()
        val roll: Float = this[2] * 180f / Math.PI.toFloat()
        return floatArrayOf(azimuth, pitch, roll)
    }

    private fun FloatArray.floatArrayToString(): String {
        return this.joinToString(separator = ",")
    }
}

@OptIn(ExperimentalHorologistApi::class)
@Composable
fun WearApp(
    viewModel: MainViewModel = viewModel(),
) {
    val uiState by viewModel.uiState.collectAsState()

    DartMapperTheme {
        val context = LocalContext.current
        val columnState = rememberColumnState()

        Scaffold(
            modifier = Modifier.background(MaterialTheme.colors.background),
            timeText = {
                TimeText()
            },
            vignette = {
                Vignette(vignettePosition = VignettePosition.TopAndBottom)
            },
            positionIndicator = {
                PositionIndicator(
                    scalingLazyListState = columnState.state
                )
            }
        ) {
            ScalingLazyColumn(
                modifier = Modifier
                    .fillMaxSize()
                    .rotaryWithScroll(columnState),
                autoCentering = AutoCenteringParams(itemIndex = 0),
                state = columnState.state
            ) {
                item {
                    Image(
                        modifier = Modifier.fillMaxWidth(),
                        painter = painterResource(id = R.drawable.tarmac),
                        contentDescription = null,
                    )
                }
                item {
                    IpTextField(
                        ip = uiState.ip,
                        setIp = { ip ->
                            viewModel.setIp(context, ip)
                        },
                    )
                }
            }
        }
    }
}

@Preview(device = Devices.WEAR_OS_SMALL_ROUND, showSystemUi = true)
@Composable
fun DefaultPreview() {
    WearApp()
}