namespace time

pack TimeInformation {
	seconds: u64
	nanoseconds: u64
}

import 'C' system_clock_get_time(id: large, time: TimeInformation*)

export now(): large {
	result: TimeInformation[1]
	system_clock_get_time(0, result)

	return result[].seconds * 10000000 + result[].nanoseconds / 100
}