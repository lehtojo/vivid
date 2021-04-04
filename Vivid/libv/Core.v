import exit(code: large)

import internal_allocate(bytes: large): link

import allocate_stack(count: large): link
import deallocate_stack(count: large)

import fill(destination: link, count: large, value: large)
import zero(destination: link, count: large)

true = 1
false = 0

none = 0

PAGE_SIZE = 1000000

Page {
	address: link
	position: large
}

Allocation {
	static current: Page
}

outline allocate(bytes: large) {
	if Allocation.current != none and Allocation.current.position + bytes <= PAGE_SIZE {
		position = Allocation.current.position
		Allocation.current.position += bytes

		=> (Allocation.current.address + position) as link
	}

	address = internal_allocate(PAGE_SIZE)

	page = internal_allocate(32) as Page
	page.address = address
	page.position = bytes

	Allocation.current = page

	=> address as link
}

outline deallocate(address: link) {}

outline allocate<T>(count: large) => allocate(count * sizeof(T)) as link<T>

TYPE_DESCRIPTOR_FULLNAME_OFFSET = 0
TYPE_DESCRIPTOR_INHERITANT_SEPARATOR = 1
TYPE_DESCRIPTOR_FULLNAME_END = 2

outline inherits(descriptor: link, type: link) {
	x = (descriptor as l64)[TYPE_DESCRIPTOR_FULLNAME_OFFSET] as link
	y = (type as l64)[TYPE_DESCRIPTOR_FULLNAME_OFFSET] as link

	s = y[0]
	i = 0

	loop {
		a = x[i]
		i++

		if a == s {
			j = 1

			loop {
				a = x[i]
				b = y[j]

				i++
				j++

				if a != b and a == TYPE_DESCRIPTOR_INHERITANT_SEPARATOR and b == 0 => true
			}
		}
		else a == TYPE_DESCRIPTOR_FULLNAME_END => false
	}
}

move(source: link, offset: large, destination: link, bytes: large) {
	# Copy the area to be moved to a temporary buffer, since moving can override the bytes to be moved
	buffer = allocate(bytes)
	source += offset
	copy(source, bytes, buffer)
	
	# Copy the contents of the temporary buffer to the destination
	copy(buffer, bytes, destination)

	# Delete the temporary buffer
	deallocate(buffer)
}

move(source: link, destination: link, bytes: large) {
	# Copy the area to be moved to a temporary buffer, since moving can override the bytes to be moved
	buffer = allocate(bytes)
	copy(source, bytes, buffer)
    
	# Copy the contents of the temporary buffer to the destination
	copy(buffer, bytes, destination)

	# Delete the temporary buffer
	deallocate(buffer)
}

RangeIterator {
	start: large
	end: large
	position: large

	init(start: large, end: large) {
		this.start = start
		this.end = end
		this.position = start - 1
	}

	next() {
		=> ++position <= end
	}

	value() => position

	reset() {
		position = start - 1
	}
}

Range {
	start: large
	end: large

	init(start: large, end: large) {
		this.start = start
		this.end = end
	}

	iterator() => RangeIterator(start, end)
}