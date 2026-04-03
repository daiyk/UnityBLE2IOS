using System;
using UnityEngine;

namespace UnityBLE2IOS
{
    [Serializable]
    public class CharacteristicNotificationStateResult
    {
        public string deviceId;
        public string characteristicUUID;
        public bool isNotifying;
        public string error;

        public CharacteristicNotificationStateResult()
        {
            error = "";
        }

        public CharacteristicNotificationStateResult(string deviceId, string characteristicUUID, bool isNotifying, string error = null)
        {
            this.deviceId = deviceId;
            this.characteristicUUID = characteristicUUID;
            this.isNotifying = isNotifying;
            this.error = error ?? "";
        }

        public bool IsSuccess()
        {
            return string.IsNullOrEmpty(error);
        }

        public bool IsError()
        {
            return !string.IsNullOrEmpty(error);
        }

        public override string ToString()
        {
            if (IsSuccess())
            {
                return $"CharacteristicNotificationStateResult(Device: {deviceId}, Characteristic: {characteristicUUID}, Notifying: {isNotifying})";
            }

            return $"CharacteristicNotificationStateResult(Device: {deviceId}, Characteristic: {characteristicUUID}, Error: {error}, Notifying: {isNotifying})";
        }
    }
}
