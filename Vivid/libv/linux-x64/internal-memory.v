namespace internal

constant PERMISSION_READ = 1
constant PERMISSION_WRITE = 2

constant MAP_PRIVATE = 0x02
constant MAP_ANONYMOUS = 0x20

import 'C' system_memory_map(address: link, length: large, protection: large, flags: large, file: large, offset: large): link
import 'C' system_memory_unmap(address: link, length: large): normal

export allocate(size: large) {
	=> system_memory_map(0 as link, size, PERMISSION_READ | PERMISSION_WRITE, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0)
}

export deallocate(address: link, size: large) {
	system_memory_unmap(address, size)
}