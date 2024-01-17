package com.pezcraft.dartmapper.domain

import android.util.Log
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.net.DatagramPacket
import java.net.DatagramSocket
import java.net.InetAddress
import java.net.UnknownHostException

class UdpClient {
    private val serverPort = 4455

    suspend fun sendMessage(ip: String, message: String) {
        withContext(Dispatchers.IO) {
            val socket = DatagramSocket()
            try {
                val serverInetAddress = InetAddress.getByName(ip)
                val data = message.toByteArray()
                val packet = DatagramPacket(data, data.size, serverInetAddress, serverPort)

                socket.send(packet)
                socket.close()
            } catch (exception: UnknownHostException) {
                Log.e("UDPClient", "${exception.message}")
            }
        }
    }
}
