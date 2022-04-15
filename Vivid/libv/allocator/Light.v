import decimal_to_bits(value: decimal): large
import bits_to_decimal(value: large): decimal

import fill(destination: link, count: large, value: large)
import zero(destination: link, count: large)

import copy(source: link, bytes: large, destination: link)
import offset_copy(source: link, bytes: large, destination: link, offset: large)

none = 0

PAGE_SIZE = 10000000

export Page {
	address: link
	position: large
}

export Allocation {
	static current: Page
	static deallocations: large
}

outline allocate(bytes: large) {
	# Allocate the memory directly if it is large enough
	if bytes >= 100000 => internal.allocate(bytes)

	if Allocation.current != none and Allocation.current.position + bytes <= PAGE_SIZE {
		position = Allocation.current.position
		Allocation.current.position += bytes

		=> (Allocation.current.address + position) as link
	}

	address = internal.allocate(PAGE_SIZE)

	page = internal.allocate(32) as Page
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

export RangeIterator {
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

	value() {
		=> position
	}

	reset() {
		position = start - 1
	}
}

export Range {
	start: large
	end: large

	init(start: large, end: large) {
		this.start = start
		this.end = end
	}

	iterator() {
		=> RangeIterator(start, end)
	}
}