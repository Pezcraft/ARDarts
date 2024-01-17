package com.pezcraft.dartmapper.domain

import android.app.Application
import android.content.Context
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.viewModelScope
import com.pezcraft.dartmapper.util.DataStoreManager
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

class MainViewModel(application: Application) : AndroidViewModel(application) {

    private var _uiState = MutableStateFlow(MainUIState(application))
    val uiState: StateFlow<MainUIState> = _uiState

    init {
        viewModelScope.launch {
            setIp(
                context = application.applicationContext,
                ip = DataStoreManager.getIp(application.applicationContext),
            )
        }
    }

    fun setIp(context: Context, ip: String) {
        _uiState.value = _uiState.value.copy(
            ip = ip
        )

        viewModelScope.launch {
            DataStoreManager.setIp(context, ip ?: "")
        }
    }
}

data class MainUIState(
    val context: Context,
    val ip: String = "",
)