import exit(code: large)

import decimal_to_bits(value: decimal): large
import bits_to_decimal(value: large): decimal

import internal_allocate(bytes: large): link
import deallocate(address: link, bytes: large)

import allocate_stack(count: large): link
import deallocate_stack(count: large)

import fill(destination: link, count: large, value: large)
import zero(destination: link, count: large)

import copy(source: link, bytes: large, destination: link)
import offset_copy(source: link, bytes: large, destination: link, offset: large)

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

outline deallocate(address: link) {
	address = none as link
}

outline allocate<T>(count: large) => allocate(count * sizeof(T)) as link<T>

TYPE_DESCRIPTOR_FULLNAME_OFFSET = 0
TYPE_DESCRIPTOR_FULLNAME_END = 1

outline internal_is(inspected: link, inheritant: link) {
	if inspected == inheritant => true
	
	inspected_fullname = inspected.(link<large>)[TYPE_DESCRIPTOR_FULLNAME_OFFSET] as link
	inheritant_fullname = inheritant.(link<large>)[TYPE_DESCRIPTOR_FULLNAME_OFFSET] as link
	
	# Determine the length of the name of the inheritant type
	inheritant_name_length = 0
	loop (inheritant_fullname[inheritant_name_length] != 0) { inheritant_name_length++ }

	position = 0
	length = 0

	loop {
		value = inspected_fullname[position]
		if value == TYPE_DESCRIPTOR_FULLNAME_END => false
		
		if value == 0 {
			# Ensure the names have the same length
			if length == inheritant_name_length {
				i = position - length
				j = 0

				loop (j < length) {
					# Set index i to -1 if the names differ
					if inspected_fullname[i] != inheritant_fullname[j] {
						i = -1
						stop
					}

					# Move to the next character
					i++
					j++
				}

				# If the names differ, index i must be -1
				if i != -1 => true
			}
			
			position++
			length = 0
			continue
		}

		position++
		length++
	}

	=> false
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

# Summary: Allocates a new buffer, with the size of 'to' bytes, and copies the contents of the source buffer to the new buffer. Also deallocates the source buffer.
resize(source: link, from: large, to: large) {
	resized = allocate(to)
	copy(source, min(from, to), resized)
	deallocate(source)
	=> resized
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

