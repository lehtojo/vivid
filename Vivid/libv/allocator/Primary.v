import decimal_to_bits(value: decimal): large
import bits_to_decimal(value: large): decimal

import fill(destination: link, count: large, value: large)
import zero(destination: link, count: large)

import copy(source: link, bytes: large, destination: link)
import offset_copy(source: link, bytes: large, destination: link, offset: large)

none = 0

namespace internal.allocator {
	# Memory overhead for each bucket (we ignore other insignificant overhead):
	# 1/8 * n / (1/8 * n + sn) where n is the number of slots and s is the size of each slot
	# = n / (n + 8sn)
	# = n / (n(1 + 8s))
	# = 1 / (1 + 8s) (not dependent on the number of slots)
	# Single selection buckets: 16, 32, 64, 128 (Memory overheads: ~0,8%, ~0,3%, ~0,2%, less than 0,1%)
	# Sequence buckets: 1024 (max 0xFFFF selections) => 1024 B - 67 MB
	# Otherwise, direct allocations (very small overhead)

	constant BUCKET_CAPACITY = 1000000
	constant MAX_CACHED_AVALABLE_ITEMS = 100000 # Must be less than 256 (cache size is a 8-bit integer)

	constant MAX_SEQUENCES = 100000
	constant MAX_SEQUENCE_LENGTH = 0xFFFF
	constant MAX_AVALABLE_SEQUENCES = 1000
	constant SEQUENCE_BUCKET_CAPACITY = 102400000 # 1024 * MAX_SEQUENCES (must be greater than 10MB)

	constant MIN_ALLOCATIONS_BEFORE_DISPOSE = 50000

	export panic(message: link) {
		#internal.console.write(message, length_of(message))
		application.exit(1)
	}

	# Lets the user allocate only one item at a time
	export plain SingleSelectionBucket<T> {
		# Stores whether each item is selected or not using one bit per item
		states: byte[BUCKET_CAPACITY / 8]

		# Stores available item indices as a cache 
		cache: normal[MAX_CACHED_AVALABLE_ITEMS]

		# Stores the number of available items in the cache
		cache_size: normal = 0

		# Tells the amount of items in the bucket
		# Useful for determining when the entire bucket is full or empty
		items: large = 0

		# Stores the number of times this bucket has been used to allocate. This is used to give buckets a minimum lifetime in order to prevent buckets being created and destroyed immediately
		allocations: u64 = 0

		# Stores the memory address of the first item
		start: link

		# Computes the end address of the bucket
		end => start + BUCKET_CAPACITY * capacityof(T)

		init(start: link) {
			zero(this as link, capacityof(SingleSelectionBucket<T>))
			this.start = start
		}

		# Tries to return the next available slot
		allocate(bytes: large) {
			if cache_size <= 0 fill_cache()
			if cache_size <= 0 => none as link

			index = cache[--cache_size]

			# Reserve the slot by enabling the bit
			state_index = index / 8
			state_bit = index - state_index * 8 # index % 8
			states[state_index] |= (1 <| state_bit)

			items++
			allocations++

			slot = start + index * capacityof(T)
			zero(slot, capacityof(T))
			=> slot
		}

		# Fills the cache with available items
		fill_cache() {
			if items == BUCKET_CAPACITY return

			# Bytes: BUCKET_CAPACITY / 8
			# Larges: BUCKET_CAPACITY / 8 / 8 = BUCKET_CAPACITY / 64
			loop (i = 0, i < BUCKET_CAPACITY / 64, i++) {
				# Get the state of the next eight slots
				state = states.(link<large>)[i]

				# Skip if all the slots are full
				if (!state) == 0 continue

				# Check if all of the slots are available
				if state == 0 {
					n = MAX_CACHED_AVALABLE_ITEMS - cache_size
					if n > 64 { n = 64 }

					# Add all 64 slots to the cache
					loop (j = 0, j < n, j++) {
						cache[cache_size++] = i * 64 + j
					}

					continue
				}

				# Compute the maximum amount of items we can from the state
				n = MAX_CACHED_AVALABLE_ITEMS - cache_size
				if n > 64 { n = 64 }
				j = 0
				mask = 1

				# Go through the individual bits
				loop (j < n) {
					# Check if the slot is available
					if (state & mask) == 0 {
						# Add the item to the cache
						cache[cache_size++] = i * 64 + j
					}

					# Move to the next slot
					mask = mask <| 1
					j++
				}
			}
		}

		# Deallocates the specified slot
		deallocate(slot: link) {
			# Compute the index of the slot
			offset = slot - start
			index = offset / capacityof(T)

			# Ensure the specified slot address points exactly to the start of the slot
			remainder = offset - index * capacityof(T)
			if remainder != 0 panic('Address did not point to the start of an allocated area')

			state_index = index / 8 # Compute the index of the state, which contains the bit for the deallocated item
			state_bit = index - state_index * 8 # Compute the index of the bit in the state that corresponds to the deallocated item

			state = states[state_index]
			mask = 1 <| state_bit
			if (state & mask) == 0 panic('Address was deallocated twice')

			items-- # Decrement the number of items in the bucket
			states[state_index] = state & (!mask) # Disable the bit

			# Add the slot to the cache
			if cache_size < MAX_CACHED_AVALABLE_ITEMS {
				cache[cache_size++] = index
			}
		}

		dispose() {
			internal.deallocate(this.start, BUCKET_CAPACITY * capacityof(T))
		}
	}

	export pack Sequence {
		start: normal
		size: normal
	}

	# Lets the user allocate multiple items at a time
	export plain SequenceBucket<T> {
		# Each state stores the offset to the next available slot or to the start of the next sequence
		states: normal[MAX_SEQUENCES]

		# Stores starting indices of sequences, which can be used for allocation
		available: Sequence[MAX_AVALABLE_SEQUENCES]
		available_items: normal = 0

		# Tells the amount of items in the bucket
		# Useful for determining when the entire bucket is full or empty
		items: large = 0

		# Stores the number of times this bucket has been used. This is used to give buckets a minimum lifetime in order to prevent buckets being created and destroyed immediately
		allocations: u64 = 0

		# Stores the memory address of the first item
		start: link

		# Computes the end address of the bucket
		end => start + MAX_SEQUENCES * capacityof(T)

		init(start: link) {
			zero(this as link, capacityof(SequenceBucket<T>))
			this.start = start
		}

		# Tries to allocate the specified amount of bytes using a single or multiple slots
		allocate(bytes: large) {
			if items == MAX_SEQUENCES => none as link

			# Compute how many slots are needed to store the specified amount of bytes
			slots = (bytes + capacityof(T) - 1) / capacityof(T)
			sequence_index = find_suitable_sequence(slots)

			# If there is no suitable sequence, try to load more available sequences anc check again
			if sequence_index < 0 {
				if available_items == MAX_AVALABLE_SEQUENCES => none as link

				# Load more sequences
				fill_available_sequences()

				# Try again
				sequence_index = find_suitable_sequence(slots)
				if sequence_index < 0 => none as link
			}

			sequence = available[sequence_index]
			first = sequence.start

			if sequence.size == slots {
				available_items--

				# Remove the sequence from the available list
				loop (i = sequence_index, i < available_items, i++) {
					available[i] = available[i + 1]
				}
			}
			else {
				sequence.start += slots
				sequence.size -= slots
				available[sequence_index] = sequence
			}

			# Store the offset to the end of the sequence into each slot
			loop (i = 0, i < slots, i++) {
				states[first + i] = slots - i
			}

			items += slots # Increment the number of items in the bucket
			allocations += slots # Increment the number of allocations in the bucket

			address = start + first * capacityof(T)
			zero(address, bytes)
			=> address
		}

		# Summary: Returns the index of the first available sequence, which can contains the specified amount of slots. Returns -1 if no suitable sequence is found.
		find_suitable_sequence(slots: byte) {
			loop (i = 0, i < available_items, i++) {
				sequence_size = available[i].size
				# If the sequence is large enough, return it
				if sequence_size >= slots => i
			}

			=> -1
		}

		# Summary: Finds the starting indices of available sequences and stores them into cache
		fill_available_sequences() {
			i = 0

			# Fix magic constant
			loop (i < MAX_SEQUENCES and available_items < MAX_AVALABLE_SEQUENCES) {
				state = states[i]

				if state != 0 {
					i += state # Since this is a start of an allocated sequence, we can skip to the end of it
					continue
				}

				# Save the start of this available sequence
				sequence: Sequence
				sequence.start = i
				sequence.size = 1
				i++

				# Skip to the next sequence and determine the length of the current available sequence
				loop (i < MAX_SEQUENCES, i++) {
					state = states[i]
					if state != 0 stop
					sequence.size++
				}

				available[available_items] = sequence
				available_items++
			}
		}

		deallocate(slot: link) {
			# Compute the index of the slot
			offset = slot - start
			index = offset / capacityof(T)

			# Ensure the specified slot address points exactly to the start of the slot
			remainder = offset - index * capacityof(T)
			if remainder != 0 panic('Address did not point to the start of an allocated area')

			# This is the first slot of a sequence
			# 1. If this slot is the first slot in the entire bucket
			# 2. If the value in the previous slot is either 0 or 1
			if index != 0 and states[index - 1] > 1 panic('Address did not point to the start of an allocated area')

			# Extract the length of the sequence
			length = states[index]

			# Deallocate each slot in the sequence
			loop (i = 0, i < length, i++) {
				states[index + i] = 0
			}

			# Add the sequence to the available list
			if available_items < MAX_AVALABLE_SEQUENCES {
				sequence: Sequence
				sequence.start = index
				sequence.size = length

				available[available_items] = sequence
				available_items++
			}

			items -= length
		}

		dispose() {
			internal.deallocate(this.start, MAX_SEQUENCES * capacityof(T))
		}
	}

	export plain Allocators<T, B> {
		allocation: link<T> # Buckets sorted for allocation (most allocated bucket first)
		deallocation: link<T> # Buckets sorted for deallocation (sorted by addresses)
		size: large = 0 # Number of buckets
		capacity: large = 0 # Stores the size of each list
		low: link = 0x7fffffffffffffff as link
		high: link = 0 as link

		allocate(bytes: large) {
			loop (i = size - 1, i >= 0, i--) {
				address = allocation[i].allocate(bytes)
				if address != none => address
			}

			new = add()
			=> new.allocate(bytes)
		}

		add() {
			start = internal.allocate(capacityof(B))
			end = start + capacityof(B)
			if start == none panic('Out of memory')

			# Update the low and high addresses, used for finding the correct allocator list for deallocation
			if start < low { low = start }
			if end > high { high = end }

			# Extend each of the list if needed
			if size + 1 > capacity {
				extended_allocation = internal.allocate((capacity + 1) * 2 * capacityof(T))
				extended_deallocation = internal.allocate((capacity + 1) * 2 * capacityof(T))
				copy(allocation, capacity * capacityof(T), extended_allocation)
				copy(deallocation, capacity * capacityof(T), extended_deallocation)
				internal.deallocate(allocation, capacity * capacityof(T))
				internal.deallocate(deallocation, capacity * capacityof(T))
				capacity = (capacity + 1) * 2
				allocation = extended_allocation
				deallocation = extended_deallocation
			}

			allocator = internal.allocate(capacityof(T)) as T
			allocator.init(start)
			allocation[size] = allocator # We can just add this allocator to the end, since there are the least allocated buckets

			# Find the index where we need to insert the allocator in the deallocation list (it is sorted by the start address)
			i = 0
			loop (i < size and deallocation[i].start < start, i++) {}

			# If we are adding the allocator to the end of the deallocation list, no need to relocate any other buckets
			if i == size {
				deallocation[size] = allocator
				size++
				=> allocator
			}

			# Since we are inserting the allocator into the middle of the list, we need to move all the buckets starting from the destination index one index forward
			end = deallocation as link + i * capacityof(T)

			loop (address = deallocation as link + size * capacityof(T) - 1, address >= end, address--) {
				address[1] = address[0]
			}

			# Now, insert the allocator into the deallocation list
			deallocation[i] = allocator
			size++
			=> allocator
		}

		# Summary: Uses binary search to find the bucket, which contains the specified address
		find_bucket(address: link, left: large, right: large) {
			if right < left => -1

			middle = (left + right) / 2

			# Return the middle bucket, if it contains the address
			if deallocation[middle].start <= address and deallocation[middle].end > address => middle

			# Search all the buckets to the left of the middle bucket, if the address is to the left of the middle bucket
			if address < deallocation[middle].start => find_bucket(address, left, middle - 1) as large

			# Search all the buckets to the right of the middle bucket
			=> find_bucket(address, middle + 1, right) as large
		}

		# Summary: Uses binary search to find the bucket, which contains the specified address
		find_bucket(address: link) {
			=> find_bucket(address, 0, size)
		}

		deallocate(address: link) {
			# Since the buckets are sorted by addresses, we can use binary search to find the bucket that contains the specified slot
			bucket_index = find_bucket(address)
			if bucket_index == -1 => false

			# Deallocate the slot in the bucket
			bucket = deallocation[bucket_index]
			bucket.deallocate(address)

			# If the bucket is now empty and it is used enough times, dispose it
			if bucket.items > 0 or bucket.allocations < MIN_ALLOCATIONS_BEFORE_DISPOSE => true

			# Remove the bucket from the deallocation list
			loop (i = bucket_index, i < size - 1, i++) {
				deallocation[i] = deallocation[i + 1]
			}

			bucket_index = -1

			# Find the bucket from the allocation list
			loop (i = 0, i < size, i++) {
				if allocation[i] != bucket continue
				bucket_index = i
				stop
			}

			# Remove the bucket from the allocation list
			loop (i = bucket_index, i < size - 1, i++) {
				allocation[i] = allocation[i + 1]
			}

			size--

			# Update the low and high addresses
			if size > 0 {
				low = deallocation[0].start
				high = deallocation[size - 1].end
			}
			else {
				low = 0x7fffffffffffffff as link
				high = 0 as link
			}

			# Dispose the bucket
			bucket.dispose()
			=> true
		}
	}

	s1: Allocators<SingleSelectionBucket<byte[16]>, byte[16000000]> = none as Allocators<SingleSelectionBucket<byte[16]>, byte[16000000]> # 16 * BUCKET_CAPACITY = 16 * 1000000 = 16000000
	s2: Allocators<SingleSelectionBucket<byte[32]>, byte[32000000]> = none as Allocators<SingleSelectionBucket<byte[32]>, byte[32000000]> # 32 * BUCKET_CAPACITY = 32 * 1000000 = 32000000
	s3: Allocators<SingleSelectionBucket<byte[64]>, byte[64000000]> = none as Allocators<SingleSelectionBucket<byte[64]>, byte[64000000]> # 64 * BUCKET_CAPACITY = 64 * 1000000 = 64000000
	s4: Allocators<SingleSelectionBucket<byte[128]>, byte[128000000]> = none as Allocators<SingleSelectionBucket<byte[128]>, byte[128000000]> # 128 * BUCKET_CAPACITY = 128 * 1000000 = 128000000
	m1: Allocators<SequenceBucket<byte[1024]>, byte[SEQUENCE_BUCKET_CAPACITY]> = none as Allocators<SequenceBucket<byte[1024]>, byte[SEQUENCE_BUCKET_CAPACITY]>

	export initialize() {
		s1 = internal.allocate(capacityof(Allocators<SingleSelectionBucket<byte[16]>, byte[16000000]>)) as Allocators<SingleSelectionBucket<byte[16]>, byte[16000000]>
		s2 = internal.allocate(capacityof(Allocators<SingleSelectionBucket<byte[32]>, byte[32000000]>)) as Allocators<SingleSelectionBucket<byte[32]>, byte[32000000]>
		s3 = internal.allocate(capacityof(Allocators<SingleSelectionBucket<byte[64]>, byte[64000000]>)) as Allocators<SingleSelectionBucket<byte[64]>, byte[64000000]>
		s4 = internal.allocate(capacityof(Allocators<SingleSelectionBucket<byte[128]>, byte[128000000]>)) as Allocators<SingleSelectionBucket<byte[128]>, byte[128000000]>
		m1 = internal.allocate(capacityof(Allocators<SequenceBucket<byte[1024]>, byte[SEQUENCE_BUCKET_CAPACITY]>)) as Allocators<SequenceBucket<byte[1024]>, byte[SEQUENCE_BUCKET_CAPACITY]>
		s1.init()
		s2.init()
		s3.init()
		s4.init()
		m1.init()
	}
}

# Order buckets using insertion sort combined with merge sort
export outline allocate(bytes: large) {
	if bytes <= 16 => internal.allocator.s1.allocate(bytes)
	if bytes <= 32 => internal.allocator.s2.allocate(bytes)
	if bytes <= 64 => internal.allocator.s3.allocate(bytes)
	if bytes <= 128 => internal.allocator.s4.allocate(bytes)
	if bytes <= internal.allocator.MAX_SEQUENCE_LENGTH * 1024 => internal.allocator.m1.allocate(bytes)

	bytes += sizeof(large)
	address = internal.allocate(bytes)
	if address == none internal.allocator.panic('Out of memory')

	# Store the size of the allocation at the beginning of the allocated memory
	address.(link<large>)[0] = bytes
	=> address + sizeof(large)
}

export outline deallocate(address: link) {
	# Compute minimum and maximum address in each allocator list
	if address >= internal.allocator.s1.low and address <= internal.allocator.s1.high and internal.allocator.s1.deallocate(address) return
	if address >= internal.allocator.s2.low and address <= internal.allocator.s2.high and internal.allocator.s2.deallocate(address) return
	if address >= internal.allocator.s3.low and address <= internal.allocator.s3.high and internal.allocator.s3.deallocate(address) return
	if address >= internal.allocator.s4.low and address <= internal.allocator.s4.high and internal.allocator.s4.deallocate(address) return
	if address >= internal.allocator.m1.low and address <= internal.allocator.m1.high and internal.allocator.m1.deallocate(address) return

	# Load the size of the allocation and deallocate the memory
	address -= sizeof(large)
	bytes = address.(link<large>)[0]
	internal.deallocate(address, bytes)
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