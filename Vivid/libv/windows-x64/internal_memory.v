namespace internal

constant MEMORY_COMMIT = 0x1000
constant MEMORY_RESERVE = 0x2000
constant MEMORY_RELEASE = 0x8000

constant PAGE_READWRITE = 0x04

import 'C' VirtualAlloc(address: link, size: large, type: large, protect: bool): link
import 'C' VirtualFree(address: link, size: large, type: large)

export allocate(size: large) {
	=> VirtualAlloc(0, size, MEMORY_COMMIT | MEMORY_RESERVE, PAGE_READWRITE)
}

export deallocate(address: link) {
	=> VirtualFree(address, 0, MEMORY_RELEASE)
}