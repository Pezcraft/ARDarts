package com.pezcraft.dartmapper.components

import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.OutlinedTextFieldDefaults
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.input.KeyboardType

@Composable
fun IpTextField(
    ip: String,
    setIp: (String) -> Unit,
) {
    val text = remember { mutableStateOf(ip) }
    text.value = ip   //needed for remotely changing the text field (for example by Datastore)

    OutlinedTextField(
        value = text.value,
        onValueChange = { input ->
            text.value = input
            if (input.isBlank()) {
                setIp("")
            } else {
                setIp(input)
            }
        },
        label = {
            Text(
                text = "IP",
                color = Color.White,
            )
        },
        colors = OutlinedTextFieldDefaults.colors(
            focusedTextColor = Color.White,
            unfocusedTextColor = Color.White,
            cursorColor = Color.White,
            focusedBorderColor = Color.White,
            unfocusedBorderColor = Color.White
        ),
        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Text),
        singleLine = true,
    )
}