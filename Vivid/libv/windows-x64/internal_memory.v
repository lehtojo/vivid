namespace internal

constant MEMORY_COMMIT = 0x00001000
constant MEMORY_RESERVE = 0x00002000
constant MEMORY_RELEASE = 0x00008000

constant PAGE_EXECUTE_READWRITE = 0x04

import 'C' VirtualAlloc(address: link, size: large, type: large, protect: bool): link
import 'C' VirtualFree(address: link, size: large, type: large)

export allocate(size: large) {
	=> VirtualAlloc(none, size, MEMORY_COMMIT | MEMORY_RESERVE, PAGE_EXECUTE_READWRITE)
}

export deallocate(address: link, size: large) {
	=> VirtualFree(address, size, MEMORY_RELEASE)
}