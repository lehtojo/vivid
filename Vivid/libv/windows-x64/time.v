namespace time

import 'C' GetSystemTimeAsFileTime(result: link<large>)

export now() {
	value: large[1]
	GetSystemTimeAsFileTime(value)
	=> value[0]
}