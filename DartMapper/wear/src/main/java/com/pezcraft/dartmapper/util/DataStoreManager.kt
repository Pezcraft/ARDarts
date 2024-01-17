package com.pezcraft.dartmapper.util

import android.content.Context
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.emptyPreferences
import androidx.datastore.preferences.preferencesDataStore
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.catch
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.flow.map
import java.io.IOException

object DataStoreManager {
    private val Context.dataStore by preferencesDataStore(
        name = PREFERENCES_NAME
    )

    suspend fun getIp(context: Context): String =
        getStringValue(context, PreferencesKeys.IP, "")

    suspend fun setIp(context: Context, ip: String) =
        setStringValue(context, PreferencesKeys.IP, ip)



    private suspend fun setStringValue(context: Context, key: Preferences.Key<String>, value: String) {
        context.dataStore.edit { preferences ->
            preferences[key] = value
        }
    }

    private suspend fun setIntValue(context: Context, key: Preferences.Key<Int>, value: Int) {
        context.dataStore.edit { preferences ->
            preferences[key] = value
        }
    }

    private suspend fun setDoubleValue(context: Context, key: Preferences.Key<Double>, value: Double) {
        context.dataStore.edit { preferences ->
            preferences[key] = value
        }
    }

    private suspend fun setLongValue(context: Context, key: Preferences.Key<Long>, value: Long) {
        context.dataStore.edit { preferences ->
            preferences[key] = value
        }
    }

    private suspend fun setBooleanValue(context: Context, key: Preferences.Key<Boolean>, value: Boolean) {
        context.dataStore.edit { preferences ->
            preferences[key] = value
        }
    }



    private suspend fun getStringValue(context: Context, key: Preferences.Key<String>, default: String): String =
        context.dataStore.data
            .catch { exception ->
                if (exception is IOException) {
                    emit(emptyPreferences())
                } else {
                    throw exception
                }
            }
            .map { preferences ->
                preferences[key] ?: default
            }
            .first()

    private suspend fun getIntValue(context: Context, key: Preferences.Key<Int>, default: Int): Int =
        context.dataStore.data
            .catch { exception ->
                if (exception is IOException) {
                    emit(emptyPreferences())
                } else {
                    throw exception
                }
            }
            .map { preferences ->
                preferences[key] ?: default
            }
            .first()

    private suspend fun getDoubleValue(context: Context, key: Preferences.Key<Double>, default: Double): Double =
        context.dataStore.data
            .catch { exception ->
                if (exception is IOException) {
                    emit(emptyPreferences())
                } else {
                    throw exception
                }
            }
            .map { preferences ->
                preferences[key] ?: default
            }
            .first()

    private suspend fun getLongValue(context: Context, key: Preferences.Key<Long>, default: Long): Long =
        context.dataStore.data
            .catch { exception ->
                if (exception is IOException) {
                    emit(emptyPreferences())
                } else {
                    throw exception
                }
            }
            .map { preferences ->
                preferences[key] ?: default
            }
            .first()

    private suspend fun getBooleanValue(context: Context, key: Preferences.Key<Boolean>, default: Boolean): Boolean =
        context.dataStore.data
            .catch { exception ->
                if (exception is IOException) {
                    emit(emptyPreferences())
                } else {
                    throw exception
                }
            }
            .map { preferences ->
                preferences[key] ?: default
            }
            .first()



    private fun observeStringValue(context: Context, key: Preferences.Key<String>, default: String): Flow<String> =
        context.dataStore.data
            .catch { exception ->
                if (exception is IOException) {
                    emit(emptyPreferences())
                } else {
                    throw exception
                }
            }
            .map { preferences ->
                preferences[key] ?: default
            }

    private fun observeIntValue(context: Context, key: Preferences.Key<Int>, default: Int): Flow<Int> =
        context.dataStore.data
            .catch { exception ->
                if (exception is IOException) {
                    emit(emptyPreferences())
                } else {
                    throw exception
                }
            }
            .map { preferences ->
                preferences[key] ?: default
            }

    private fun observeBooleanValue(context: Context, key: Preferences.Key<Boolean>, default: Boolean): Flow<Boolean> =
        context.dataStore.data
            .catch { exception ->
                if (exception is IOException) {
                    emit(emptyPreferences())
                } else {
                    throw exception
                }
            }
            .map { preferences ->
                preferences[key] ?: default
            }
}