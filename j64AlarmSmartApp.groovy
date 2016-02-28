/**
 *  j64Alarm
 *
 *  Copyright 2016 Joe Jarvis
 *
 *  Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except
 *  in compliance with the License. You may obtain a copy of the License at:
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
 *  on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License
 *  for the specific language governing permissions and limitations under the License.
 *
 *  Last Updated : 1/15/2016
 *
 */
definition(
	name: "j64 Alarm",
	namespace: "j64",
	author: "Joe Jarvis",
	description: "Control your envisalink alarm using j64 Alarm Server",
	category: "SmartThings Labs",
        iconUrl: "http://cdn.device-icons.smartthings.com/Home/home3-icn.png",
        iconX2Url: "http://cdn.device-icons.smartthings.com/Home/home3-icn@2x.png",
        iconX3Url: "http://cdn.device-icons.smartthings.com/Home/home3-icn@3x.png"
)

mappings {
    path("/installDevices") {
        action: [
            POST: "installDevices",
        ]
    }
    path("/UpdatePartition") {
        action: [
            POST: "updatePartition"
        ]
    }
    path("/UpdateZone") {
        action: [
            POST: "updateZone"
        ]
    }
}

/* *************** */
/* Install Methods */
/* *************** */
def installDevices() {
    state.j64Server = params.j64Server
    state.j64Port = params.j64Port
    state.j64User = params.j64UserName
    state.j64Password = params.j64Password
    
    hubApiGet("/api/AlarmSystem")
}

def installAllDevices(partitions, zones) {
	def children = getChildDevices()

	// Add any partitions that have not already been created
	partitions.each { p -> 
		def networkId = "partition" + p.Id
        def partitionDevice = children.find { item -> item.device.deviceNetworkId == networkId }
        def name = "j64:" + p.Name + " Partition"

		if (partitionDevice == null) {
        	log.debug "Add Partition: ${name} => ${networkId}"
	        partitionDevice = addChildDevice("j64", "j64 Partition", networkId, null, [name: "${name}", label:"${name}"])
		}
        else {
        	// Try to update the name of the partition
			partitionDevice.name = name
            partitionDevice.label = name
        }
            
        // always set the current status for the partition
        partitionDevice.setAlarm(p.InAlarm, p.IsArmed)
        partitionDevice.setMode(p.ArmingMode, p.ReadyToArm)
        
        // **
        // Add an alarm device for this partition
        // **
		networkId = "alarm" + p.Id
        def alarmDevice = children.find { item -> item.device.deviceNetworkId == networkId }
        name = "j64:" + p.Name + " Alarm"

		if (alarmDevice == null) {
        	log.debug "Add Alarm: ${name} => ${networkId}"
	        alarmDevice = addChildDevice("j64", "j64 Alarm", networkId, null, [name: "${name}", label:"${name}"])
		}
        else {
        	// Try to update the name of the alarm
			alarmDevice.name = name
            alarmDevice.label = name
        }
            
        // always set the current status for the alarm
        alarmDevice.setAlarm(p.InAlarm)
	}
    
    zones.each { z ->
		def networkId = "zone" + z.Id
        def zoneDevice = children.find { item -> item.device.deviceNetworkId == networkId }
        def name = "j64:" + z.Name
        def zoneType = "j64 Contact Zone"
        if ( z.ZoneType == 1 )
           zoneType = "j64 Motion Zone"
        
        if (zoneDevice == null) {
        	log.debug "Add Zone: ${name} => ${networkId}"
	        zoneDevice = addChildDevice("j64", zoneType, networkId, null, [name: "${name}", label:"${name}"])
        }    
        else {
        	// Try to update the name of the partition
			zoneDevice.name = name
            zoneDevice.label = name
            
            // Cannot change the device type via the API
            //zoneDevice.typeName = zoneType
        }
        
        // set the current status for the zone
        zoneDevice.setState(z.Status)
    }
    
    // Delete any Zones and Parttions that are not in the alarm system configuration
    children.each { d ->
    
    	if ( d.device.deviceNetworkId.startsWith("zone") ) {
			def deviceZoneId = d.device.deviceNetworkId.replaceAll("zone","")
    		def alarmSystemZone = zones.find { z -> "${z.Id}" == "${deviceZoneId}" }
            if (alarmSystemZone == null) {
               log.debug "Removing zone: ${d.deviceNetworkId} because it was not in the j64 alarm system config"
               deleteChildDevice(d.device.deviceNetworkId)
        	}
        }
	    
    	if ( d.device.deviceNetworkId.startsWith("partition") ) {
			def devicePartitionId = d.device.deviceNetworkId.replaceAll("partition","")
    		def alarmSystemPartition = partitions.find { z -> "${z.Id}" == "${devicePartitionId}" }
            if (alarmSystemPartition == null) {
               log.debug "Removing partition: ${d.deviceNetworkId} because it was not in the j64 alarm system config"
               deleteChildDevice(d.device.deviceNetworkId)
        	}
        }
	}
}

/* ***************** */
/* Partition Methods */
/* ***************** */
def updatePartition(evt) {
    def Id = params.Id
	def Name = params.Name
	def ReadyToArm = params.ReadyToArm
    def IsArmed = params.IsArmed
    def AlarmOn = params.AlarmOn
    def ArmingMode = params.ArmingMode

	def children = getChildDevices()
    def networkId = "partition${Id}"
    def partitionDevice = children.find { item -> item.device.deviceNetworkId == networkId }
    if (partitionDevice != null) {
		log.debug "updatePartition: ${Id} ${Name} ReadyToArm: ${ReadyToArm} IsArmed: ${IsArmed} AlarmOn: ${AlarmOn} ArmingMode: ${ArmingMode}"
		partitionDevice.setAlarm(AlarmOn, IsArmed)
        partitionDevice.setMode(ArmingMode, ReadyToArm)
        
        // Keep smart home monitor (SHM) in synch with the j64 alarm server
        // We sync SHM with partition 1 in the alarm system since it does
        // not support multiple partitions
        if ("${Id}" == "1") {
        	if ("${IsArmed}".toLowerCase() == "false") {
				setShmAlarmMode("off")
			} else {
                log.debug "check ArmingMode ${ArmingMode}"
        		if (ArmingMode == "Away")
            		setShmAlarmMode("away")
        	    
                if (ArmingMode == "Stay")
    	        	setShmAlarmMode("stay")
	        }
        }
	}
    
    networkId = "alarm${Id}"
    def alarmDevice = children.find { item -> item.device.deviceNetworkId == networkId }
    if (alarmDevice != null) {
		log.debug "updateAlarm: ${Id} ${Name} AlarmOn: ${AlarmOn}"
		alarmDevice.setAlarm(AlarmOn)
	}
}

def armPartition(partitionId, stayAway) {
	if (stayAway == "away")
	    hubApiGet("/api/AlarmSystem/AwayArm/${partitionId}")
	else
    	hubApiGet("/api/AlarmSystem/StayArm/${partitionId}")
}

def disarmPartition(partitionId) {
   	hubApiGet("/api/AlarmSystem/Disarm/${partitionId}")
}

def soundAlarm(partitionId) {
    hubApiGet("/api/AlarmSystem/SoundAlarm")
}

def refreshPartition(partitionId) {
    hubApiGet("/api/AlarmSystem")
}

/* ************ */
/* Zone Methods */
/* ************ */
def updateZone(evt) {
    def Id = params.Id
    def Name = params.Name
	def Status = params.Status
    
	def children = getChildDevices()
    def networkId = "zone${Id}"
    def zoneDevice = children.find { item -> item.device.deviceNetworkId == networkId }
    if (zoneDevice != null) {
		log.debug "updateZone: ${Id} ${Name}->${Status}"
    	zoneDevice.setState(Status)
	}
}

def refreshZone(zoneId) {
    hubApiGet("/api/AlarmSystem")
}

/* ************** */
/* Initialization */
/* ************** */
def installed() { 
	initialize() 
}

def updated() { 
	unsubscribe()
  	unschedule()
	initialize() 
}

def uninstalled() {
	unschedule()
}	

def initialize() {
	subscribe(location, null, localLanHandler, [filterEvents:false])
    subscribe(location, "alarmSystemStatus", shmModeChangeHandler, [filterEvents:false])
}

def refresh() {   
    hubApiGet("/api/AlarmSystem")
}

/* ************************************ */
/* Handle event from the j64AlarmServer */ 
/* on the local LAN                     */
/* ************************************ */
def localLanHandler(evt) {
	// Only handle messages from the j64AlarmServer
	def msg = parseLanMessage(evt.description)
	if (msg.json == null) {
	    return
    }
    
    def FromHost = msg.json.FromHost
    def Route = msg.json.Route
    
    if (FromHost != j64AlarmServerAddress()) {
	    return
    }
    
    if (Route == "/api/AlarmSystem")
       installAllDevices(msg.json.Response.Partitions, msg.json.Response.Zones)
}

def j64AlarmServerAddress() {
	return state.j64Server + ":" + state.j64Port
}

/* ********************************** */
/* Handle event when the mode changes */
/* ********************************** */
def shmModeChangeHandler(evt) {

	// Unfortunately we have to assume partition 1 for the smart home monitor since it does not support
    // multiple partitions.  Maybe this could change based on the location??
	def Id = 1
	def networkId = "partition${Id}"

	def partitionDevice = getChildDevices().find { item -> item.device.deviceNetworkId == networkId }
    if (partitionDevice != null) {
    	def mode = partitionDevice.currentAlarm 

		if (evt.value == "off" && mode == "armed") {
	    	disarmPartition(1)
    	}
        
        if (evt.value != "off" && mode == "disarmed") {
	    	armPartition(1, evt.value)
    	}
	}
}

private def setShmAlarmMode(name) {
    def event = [
        name:           "alarmSystemStatus",
        value:          name,
        isStateChange:  true,
        displayed:      true,
        description:    "alarm system status is ${name}",
    ]
    sendLocationEvent(event)
}

/* **************** */
/* Helper Functions */
/* **************** */
private hubApiGet(apiPath) {	

	def userpassascii = "${state.j64User}:${state.j64Password}"
	def userpass = "Basic " + userpassascii.encodeAsBase64().toString()
    
    def headers = [:] 
    headers.put("HOST", j64AlarmServerAddress())
    headers.put("Authorization", userpass)

	def result = new physicalgraph.device.HubAction(
 		   	method: "GET",
    		path: apiPath,
    		headers: headers
		)
    sendHubCommand(result)
}