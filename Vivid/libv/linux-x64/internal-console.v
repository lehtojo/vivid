namespace internal.console

constant STANDARD_INPUT_HANDLE = 0
constant STANDARD_OUTPUT_HANDLE = 1

import 'C' system_write(handle: large, buffer: link, length: large)
import 'C' system_read(handle: large, buffer: link, length: large): large

export write(bytes: link, length: large) {
	system_write(STANDARD_OUTPUT_HANDLE, bytes, length)
}

export read(bytes: link, length: large) {
	return system_read(STANDARD_INPUT_HANDLE, bytes, length)
}