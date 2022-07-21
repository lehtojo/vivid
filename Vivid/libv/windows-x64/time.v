namespace time

import 'C' GetSystemTimeAsFileTime(result: link<large>)

export now() {
	value: large[1]
	GetSystemTimeAsFileTime(value as link<large>)
	return value[0]
}