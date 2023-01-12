export allocate(bytes: i64) {}
export deallocate(address: link) {}
export internal_is(a: link, b: link) {}

BufferAllocator {
	position: link

	init(position: link) {
		this.position = position
	}

	allocate(bytes: u64): link {
		position += bytes
		return position
	}

	deallocate(address: link) {}
}

GlobalBufferAllocator {
	shared instance: BufferAllocator

	shared initialize(buffer: link) {
		instance = BufferAllocator(buffer)
	}

	shared allocate(bytes: u64): link {
		return instance.allocate(bytes)
	}

	shared deallocate(address: link) {
		return instance.deallocate(address)
	}
}

Foo {
	x: i64
	y: i64
}

init() {
	GlobalBufferAllocator.initialize(buffer1: u8[1024])

	allocator = BufferAllocator(buffer2: u8[1024])
	foo1 = Foo() using allocator
	foo2 = Foo() using GlobalBufferAllocator
	foo3 = Foo() using 0x100000

	return 0
}