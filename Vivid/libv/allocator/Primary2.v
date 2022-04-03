namespace internal.allocator {
	constant MAX_ALLOCATORS = 100000

	s16: Allocators<SlabAllocator<byte[16]>, byte[16]>
	s32: Allocators<SlabAllocator<byte[32]>, byte[32]>
	s64: Allocators<SlabAllocator<byte[64]>, byte[64]>
	s128: Allocators<SlabAllocator<byte[128]>, byte[128]>
	s256: Allocators<SlabAllocator<byte[256]>, byte[256]>
	s512: Allocators<SlabAllocator<byte[512]>, byte[512]>
	s1024: Allocators<SlabAllocator<byte[1024]>, byte[1024]>

	export panic(message: link) {
		internal.console.write(message, length_of(message))
		application.exit(1)
	}

	export plain SlabAllocator<T> {
		slabs: normal
		start: link
		end => start + slabs * capacityof(T)

		# Stores the states of all the slabs as bits. This should only be used for debugging.
		states: link

		# Stores the number of times this bucket has been used to allocate. This is used to give buckets a minimum lifetime in order to prevent buckets being created and destroyed immediately
		allocations: u64 = 0

		# Stores the number of used slots in this allocator
		used: normal = 0

		available: link = none as link
		position: normal = 0

		init(start: link, slabs: normal) {
			this.start = start
			this.slabs = slabs

			# NOTE: Debug mode only
			this.states = internal.allocate(slabs / 8) # Allocate bits for each slab
		}

		allocate() {
			if available != none {
				result = available

				# NOTE: Debug mode only
				# Set the bit for this slab
				index = (result - start) / capacityof(T)
				states[index / 8] |= 1 <| (index % 8)

				next = result.(link<link>)[0]
				available = next

				allocations++
				used++

				zero(result, capacityof(T))
				=> result
			}

			if position < slabs {
				result = start + position * capacityof(T)

				# NOTE: Debug mode only
				# Set the bit for this slab
				states[position / 8] |= 1 <| (position % 8)

				# Move to the next slab
				position++
				allocations++
				used++

				zero(result, capacityof(T))
				=> result
			}

			=> none as link
		}

		allocate_slab(index: large) {
			# Ensure the slab is not already deallocated
			mask = 1 <| (index % 8)
			state = states[index / 8] & mask

			if state == 0 panic('Address already deallocated')

			states[index / 8] ¤= mask
		}

		deallocate(address: link) {
			offset = address - start
			index = offset / capacityof(T)
			require(offset - index * capacityof(T) == 0, 'Address did not point to the start of an allocated area')

			# NOTE: Debug mode only
			# Ensure the slab is not already deallocated
			allocate_slab(index)

			address.(link<link>)[0] = available
			available = address

			used--
		}

		dispose() {
			internal.deallocate(start, slabs * capacityof(T))
			internal.deallocate(states, slabs / 8)
		}
	}

	export plain Allocators<T, S> {
		allocators: link
		size: large = 0
		slabs: normal

		init(slabs: normal) {
			this.allocators = internal.allocate(MAX_ALLOCATORS * capacityof(T))
			this.slabs = slabs
		}

		add() {
			require(size < MAX_ALLOCATORS, 'Allocator limit reached')

			memory = internal.allocate(slabs * capacityof(S))

			allocator = (allocators + size * capacityof(T)) as T
			allocator.init(memory, slabs)

			size++
			=> allocator
		}

		allocate() {
			loop (i = 0, i < size, i++) {
				allocator = (allocators + i * capacityof(T)) as T
				result = allocator.allocate()

				if result != none => result
			}

			=> add().allocate()
		}

		deallocate(address: link) {
			loop (i = 0, i < size, i++) {
				allocator = (allocators + i * capacityof(T)) as T
				if address < allocator.start or address >= allocator.end continue

				allocator.deallocate(address)

				# Deallocate the allocator if it is empty and is used long enough
				if allocator.allocations > slabs / 2 and allocator.used == 0 {
					allocator.dispose()

					# Move all allocators one to the left after the current allocator
					copy(allocators + (i + 1) * capacityof(T), (size - i - 1) * capacityof(T), allocators + i * capacityof(T))
					zero(allocators + (size - 1) * capacityof(T), capacityof(T))

					size--
				}

				=> true
			}

			=> false
		}
	}

	initialize() {
		s16 = internal.allocate(capacityof(Allocators<SlabAllocator<byte[16]>, byte[16]>)) as Allocators<SlabAllocator<byte[16]>, byte[16]>
		s32 = internal.allocate(capacityof(Allocators<SlabAllocator<byte[32]>, byte[32]>)) as Allocators<SlabAllocator<byte[32]>, byte[32]>
		s64 = internal.allocate(capacityof(Allocators<SlabAllocator<byte[64]>, byte[64]>)) as Allocators<SlabAllocator<byte[64]>, byte[64]>
		s128 = internal.allocate(capacityof(Allocators<SlabAllocator<byte[128]>, byte[128]>)) as Allocators<SlabAllocator<byte[128]>, byte[128]>
		s256 = internal.allocate(capacityof(Allocators<SlabAllocator<byte[256]>, byte[256]>)) as Allocators<SlabAllocator<byte[256]>, byte[256]>
		s512 = internal.allocate(capacityof(Allocators<SlabAllocator<byte[512]>, byte[512]>)) as Allocators<SlabAllocator<byte[512]>, byte[512]>
		s1024 = internal.allocate(capacityof(Allocators<SlabAllocator<byte[1024]>, byte[1024]>)) as Allocators<SlabAllocator<byte[1024]>, byte[1024]>

		s16.init(1000000)
		s32.init(1000000 / 2)
		s64.init(1000000 / 4)
		s128.init(1000000 / 8)
		s256.init(1000000 / 16)
		s512.init(1000000 / 32)
		s1024.init(1000000 / 64)
	}
}

export outline allocate(bytes: large) {
	if bytes <= 16 => internal.allocator.s16.allocate()
	if bytes <= 32 => internal.allocator.s32.allocate()
	if bytes <= 64 => internal.allocator.s64.allocate()
	if bytes <= 128 => internal.allocator.s128.allocate()
	if bytes <= 256 => internal.allocator.s256.allocate()
	if bytes <= 512 => internal.allocator.s512.allocate()
	if bytes <= 1024 => internal.allocator.s1024.allocate()

	bytes += sizeof(large)
	address = internal.allocate(bytes)
	if address == none internal.allocator.panic('Out of memory')

	# Store the size of the allocation at the beginning of the allocated memory
	address.(link<large>)[0] = bytes
	=> address + sizeof(large)
}


export outline deallocate(address: link) {
	if internal.allocator.s16.deallocate(address) return
	if internal.allocator.s32.deallocate(address) return
	if internal.allocator.s64.deallocate(address) return
	if internal.allocator.s128.deallocate(address) return
	if internal.allocator.s256.deallocate(address) return
	if internal.allocator.s512.deallocate(address) return
	if internal.allocator.s1024.deallocate(address) return

	# Load the size of the allocation and deallocate the memory
	address -= sizeof(large)
	bytes = address.(link<large>)[0]
	internal.deallocate(address, bytes)
}

# -----------------------------------------------------------------------------------------

import decimal_to_bits(value: decimal): large
import bits_to_decimal(value: large): decimal

import fill(destination: link, count: large, value: large)
import zero(destination: link, count: large)

import copy(source: link, bytes: large, destination: link)
import offset_copy(source: link, bytes: large, destination: link, offset: large)

none = 0

outline allocate<T>(count: large) {
	=> allocate(count * sizeof(T)) as link<T>
}

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