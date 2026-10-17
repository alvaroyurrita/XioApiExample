# Crestron XIO Cloud API WebSocket Documentation

 Get Notification Information method will request the websocket URI used here, and when run it will listen to real time notices from the XIO Cloud account.
The related event type of updates pushed by the WebSocket API is a JSON object containing an array of objects with the following key‑value pairs:

* Cid: A string that contains the unique identifier for a device.
* DeviceName: A string that contains the device name.
* AccountId: A string that contains the unique identifier of the account associated with the device.
* GroupId: A string that contains the unique identifier of the group associated with the device.
* DeviceModel: A string that contains the device model.
* Timestamp: A string that contains the time stamp of when the update occurred (in UTC format).

This event can be used to receive updates to device status changes, such as the addition of new data or updates to existing data.
Once this message has been processed, refer to Device Status (V2) for information on how to get the latest data from the updated device.

## Response Example:

```json

[
    {
        "Cid": "00-10-7f-9c-c8-61_-1",
        "DeviceName": "cp4-home",
        "AccountId": "e43a2e08-e3e6-4d0d-b375-5a63c1b0d94b",
        "GroupId": "bca30251-4f8e-4a2c-b52f-76805e9fc934",
        "DeviceModel": "CP4",
        "Timestamp": "2026-10-17T02:38:57.6706605Z"
    }
]
```

