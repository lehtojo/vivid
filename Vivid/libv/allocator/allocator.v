namespace internal.allocator {
	constant ESTIMATED_MAX_ALLOCATORS = 100000
	constant ALLOCATOR_SORT_INTERVAL = 10000

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
		end => start + slabs * sizeof(T)

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
				index = (result - start) as u64 / sizeof(T)
				states[index / 8] |= 1 <| (index % 8)

				next = result.(link*)[]
				available = next

				allocations++
				used++

				zero(result, sizeof(T))
				return result
			}

			if position < slabs {
				result = start + position * sizeof(T)

				# NOTE: Debug mode only
				# Set the bit for this slab
				states[position / 8] |= 1 <| (position % 8)

				# Move to the next slab
				position++
				allocations++
				used++

				zero(result, sizeof(T))
				return result
			}

			return none as link
		}

		allocate_slab(index: large) {
			# Ensure the slab is not already deallocated
			mask = 1 <| (index % 8)
			state = states[index / 8] & mask

			if state == 0 panic('Address already deallocated')

			states[index / 8] ¤= mask
		}

		deallocate(address: link) {
			offset = (address - start) as u64
			index = offset / sizeof(T)
			require(offset - index * sizeof(T) == 0, 'Address did not point to the start of an allocated area')

			# NOTE: Debug mode only
			# Ensure the slab is not already deallocated
			allocate_slab(index)

			address.(link*)[] = available
			available = address

			used--
		}

		dispose() {
			internal.deallocate(start, slabs * sizeof(T))
			internal.deallocate(states, slabs / 8)
		}
	}

	export plain Allocators<T, S> {
		allocations: u64 = 0
		allocators: T*
		deallocators: T*
		size: large = 0
		capacity: large = ESTIMATED_MAX_ALLOCATORS
		slabs: normal

		init(slabs: normal) {
			this.allocators = internal.allocate(ESTIMATED_MAX_ALLOCATORS * strideof(T))
			this.deallocators = internal.allocate(ESTIMATED_MAX_ALLOCATORS * strideof(T))
			this.slabs = slabs
		}

		sort_allocators() {
			sort<T>(allocators, size, (a: T, b: T) -> a.used - b.used)
		}

		add() {
			if size >= capacity {
				# Allocate new allocator and deallocator lists
				new_capacity = size * 2
				new_allocators = internal.allocate(new_capacity * strideof(T))
				new_deallocators = internal.allocate(new_capacity * strideof(T))

				# Copy the contents of the old allocator and deallocator lists to the new ones
				copy(allocators, size * strideof(T), new_allocators)
				copy(deallocators, size * strideof(T), new_deallocators)

				# Deallocate the old allocator and deallocator lists
				internal.deallocate(allocators, capacity * strideof(T))
				internal.deallocate(deallocators, capacity * strideof(T))

				capacity = new_capacity
				allocators = new_allocators
				deallocators = new_deallocators
			}

			# Create a new allocator with its own memory
			memory = internal.allocate(slabs * sizeof(S))

			allocator = internal.allocate(sizeof(T)) as T
			allocator.init(memory, slabs)

			# Add the new allocator
			allocators[size] = allocator
			deallocators[size] = allocator

			size++
			return allocator
		}

		allocate() {
			# Sort the allocators from time to time
			if (++allocations) % ALLOCATOR_SORT_INTERVAL == 0 sort_allocators()

			loop (i = 0, i < size, i++) {
				allocator = allocators[i]
				result = allocator.allocate()

				if result != none return result
			}

			return add().allocate()
		}

		remove(deallocator: T, i: large) {
			deallocator.dispose()

			# Remove deallocator from the list
			copy(deallocators + (i + 1) * strideof(T), (size - i - 1) * strideof(T), deallocators + i * strideof(T))
			zero(deallocators + (size - 1) * strideof(T), strideof(T))

			# Find the corresponding allocator from the allocator list linearly, because we can not assume the list is sorted in any way
			loop (j = 0, j < size, j++) {
				if allocators[j] != deallocator continue

				# Remove allocator from the list
				copy(allocators + (j + 1) * strideof(T), (size - j - 1) * strideof(T), allocators + j * strideof(T))
				zero(allocators + (size - 1) * strideof(T), strideof(T))
				stop
			}

			size--
		}

		deallocate(address: link) {
			loop (i = 0, i < size, i++) {
				deallocator = deallocators[i]
				if address < deallocator.start or address >= deallocator.end continue

				deallocator.deallocate(address)

				# Deallocate the allocator if it is empty and is used long enough
				if deallocator.allocations > slabs / 2 and deallocator.used == 0 {
					remove(deallocator, i)
				}

				return true
			}

			return false
		}
	}

	initialize() {
		s16 = internal.allocate(sizeof(Allocators<SlabAllocator<byte[16]>, byte[16]>)) as Allocators<SlabAllocator<byte[16]>, byte[16]>
		s32 = internal.allocate(sizeof(Allocators<SlabAllocator<byte[32]>, byte[32]>)) as Allocators<SlabAllocator<byte[32]>, byte[32]>
		s64 = internal.allocate(sizeof(Allocators<SlabAllocator<byte[64]>, byte[64]>)) as Allocators<SlabAllocator<byte[64]>, byte[64]>
		s128 = internal.allocate(sizeof(Allocators<SlabAllocator<byte[128]>, byte[128]>)) as Allocators<SlabAllocator<byte[128]>, byte[128]>
		s256 = internal.allocate(sizeof(Allocators<SlabAllocator<byte[256]>, byte[256]>)) as Allocators<SlabAllocator<byte[256]>, byte[256]>
		s512 = internal.allocate(sizeof(Allocators<SlabAllocator<byte[512]>, byte[512]>)) as Allocators<SlabAllocator<byte[512]>, byte[512]>
		s1024 = internal.allocate(sizeof(Allocators<SlabAllocator<byte[1024]>, byte[1024]>)) as Allocators<SlabAllocator<byte[1024]>, byte[1024]>

		s16.init(5000000)
		s32.init(5000000 / 2)
		s64.init(5000000 / 4)
		s128.init(5000000 / 8)
		s256.init(5000000 / 16)
		s512.init(5000000 / 32)
		s1024.init(5000000 / 64)
	}
}

export outline allocate(bytes: large) {
	if bytes <= 16 return internal.allocator.s16.allocate()
	if bytes <= 32 return internal.allocator.s32.allocate()
	if bytes <= 64 return internal.allocator.s64.allocate()
	if bytes <= 128 return internal.allocator.s128.allocate()
	if bytes <= 256 return internal.allocator.s256.allocate()
	if bytes <= 512 return internal.allocator.s512.allocate()
	if bytes <= 1024 return internal.allocator.s1024.allocate()

	bytes += strideof(large)
	address = internal.allocate(bytes)
	if address == none internal.allocator.panic('Out of memory')

	# Store the size of the allocation at the beginning of the allocated memory
	address.(large*)[] = bytes
	return address + strideof(large)
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
	address -= strideof(large)
	bytes = address.(large*)[]
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
	return allocate(count * strideof(T)) as T*
}

TYPE_DESCRIPTOR_FULLNAME_OFFSET = 0
TYPE_DESCRIPTOR_FULLNAME_END = 1

outline internal_is(inspected: link, inheritant: link) {
	if inspected == inheritant return true
	
	inspected_fullname = inspected.(large*)[TYPE_DESCRIPTOR_FULLNAME_OFFSET] as link
	inheritant_fullname = inheritant.(large*)[TYPE_DESCRIPTOR_FULLNAME_OFFSET] as link
	
	# Determine the length of the name of the inheritant type
	inheritant_name_length = 0
	loop (inheritant_fullname[inheritant_name_length] != 0) { inheritant_name_length++ }

	position = 0
	length = 0

	loop {
		value = inspected_fullname[position]
		if value == TYPE_DESCRIPTOR_FULLNAME_END return false
		
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
				if i != -1 return true
			}
			
			position++
			length = 0
			continue
		}

		position++
		length++
	}

	return false
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
		return ++position <= end
	}

	value() {
		return position
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
		return RangeIterator(start, end)
	}
}