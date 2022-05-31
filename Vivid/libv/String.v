# Summary: Tries to convert the specified string to a decimal number
export as_decimal(string: String) {
	=> as_decimal(string.data, string.length)
}

# Summary: Tries to convert the specified string to an integer number
export as_integer(string: String) {
	=> as_integer(string.data, string.length)
}

# Summary: Tries to convert the specified string to a decimal number
export to_decimal(string: String) {
	=> to_decimal(string.data, string.length)
}

# Summary: Tries to convert the specified string to an integer number
export to_integer(string: String) {
	=> to_integer(string.data, string.length)
}

# Summary: Converts the specified decimal to a string
export to_string(value: decimal) {
	buffer: byte[64]
	zero(buffer as link, 64)
	length = to_string(value, buffer as link)
	=> String(buffer as link, length)
}

# Summary: Converts the specified integer to a string
export to_string(value: large) {
	buffer: byte[32]
	zero(buffer as link, 32)
	length = to_string(value, buffer as link)
	=> String(buffer as link, length)
}

export String {
	static empty: String

	# Summary: Combines all the specified strings while separating them the specified separator
	static join(separator: char, strings: List<String>) {
		if strings.size == 0 => String.empty
		if strings.size == 1 => strings[0]

		# Set the length of the result to the number of separators, because each separator adds one character
		result_length = strings.size - 1

		# Add the lengths of all the strings to join
		loop (i = 0, i < strings.size, i++) {
			result_length += strings[i].length
		}

		# Allocate and populate the result
		buffer = allocate(result_length + 1)
		position = buffer

		loop (i = 0, i < strings.size, i++) {
			# Add the string to the result
			string = strings[i]
			copy(string.data, string.length, position)
			position += string.length

			# Add the separator, even if it is the last one
			position[0] = separator
			position++
		}

		# Remove the last separator and replace it with a zero terminator
		buffer[result_length] = 0

		=> String.from(buffer, result_length)
	}

	# Summary: Combines all the specified strings while separating them the specified separator
	static join(separator: String, strings: List<String>) {
		if strings.size == 0 => String.empty
		if strings.size == 1 => strings[0]

		# Set the length of the result to the number of characters the separators will take
		result_length = (strings.size - 1) * separator.length

		# Add the lengths of all the strings to join
		loop (i = 0, i < strings.size, i++) {
			result_length += strings[i].length
		}

		# Allocate and populate the result
		buffer = allocate(result_length + 1)

		# Add the first string to the result
		string = strings[0]
		copy(string.data, string.length, buffer)

		# Start after the first added string
		position = buffer + string.length

		loop (i = 1, i < strings.size, i++) {
			# Add the separator
			copy(separator.data, separator.length, position)
			position += separator.length

			# Add the string to the result
			string = strings[i]
			copy(string.data, string.length, position)
			position += string.length
		}

		=> String.from(buffer, result_length)
	}

	public readonly data: link
	public readonly length: large

	# Summary: Returns whether this string is empty
	empty => length == 0

	# Summary: Creates a string from the specified data. Does not copy the content of the specified data.
	static from(data: link, length: large) {
		result = String()
		result.data = data
		result.length = length
		=> result
	}

	# Summary: Converts the specified character into a string
	init(value: char) {
		data = allocate(2)
		data[0] = value
		data[1] = 0
		length = 1
	}

	# Summary: Creates a string by copying the characters from the specified source
	init(source: link) {
		a = length_of(source)
		length = a

		data = allocate(a + 1)
		data[a] = 0

		copy(source, a, data)
	}

	# Summary: Creates a string by copying the characters from the specified source using the specified length
	init(source: link, length: large) {
		this.length = length

		data = allocate(length + 1)
		data[length] = 0

		copy(source, length, data)
	}

	# Summary: Creates an empty string
	private init() {}

	# Summary: Puts the specified character into the specified position without removing any other characters and returns a new string
	insert(index: large, character: char) {
		require(index >= 0 and index <= length)
		a = length

		# Reserve memory for the current characters plus the new character and the zero byte
		memory = allocate(a + 2)

		# Copy the first segment before the index to the buffer
		copy(data, index, memory)
		# Copy the second segment after the index to the buffer, leaving space for the character
		offset_copy(data, a - index, memory, index + 1)

		# Insert the character and the terminator
		memory[index] = character
		memory[a + 1] = 0

		# Create a new string from the buffer
		result = String()
		result.data = memory
		result.length = a + 1
		=> result
	}

	# Summary: Returns whether the first characters match the specified string
	starts_with(start: String) {
		=> starts_with(start.data)
	}

	# Summary: Returns whether the first characters match the specified string
	starts_with(start: link) {
		a = length_of(start)
		if a == 0 or a > length => false

		loop (i = 0, i < a, i++) {
			if data[i] != start[i] => false
		}

		=> true
	}

	# Summary: Returns whether the first character matches the specified character
	starts_with(value: char) {
		=> length > 0 and data[0] == value
	}

	# Summary: Returns whether the last character matches the specified character
	ends_with(value: char) {
		=> length > 0 and data[length - 1] == value
	}

	# Summary: Returns whether the last characters match the specified string
	ends_with(end: link) {
		a = length_of(end)
		b = length

		if a == 0 or a > b => false

		loop (a > 0) {
			if end[--a] != data[--b] => false
		}

		=> true
	}

	# Summary: Returns the characters between the specified start and end index as a string
	slice(start: large, end: large) {
		require(start >= 0 and start <= end, 'Invalid slice start index')
		require(end <= length, 'Invalid slice end index')

		a = length
		require(start >= 0 and start <= a and end >= start and end <= a)

		=> String(data + start, end - start)
	}

	# Summary: Returns all the characters after the specified index as a string
	slice(start: large) {
		require(start >= 0 and start <= length, 'Invalid slice start index')
		=> slice(start, length)
	}

	# Summary: Replaces all the occurrences of the specified character with the specified replacement
	replace(old: char, new: char) {
		a = length
		
		result = String(data, a)
		data = result.data

		loop (i = 0, i < a, i++) {
			if data[i] != old continue
			data[i] = new
		}

		=> result
	}

	# Summary: Returns the index of the first occurrence of the specified character
	index_of(value: char) {
		a = length

		loop (i = 0, i < a, i++) {
			if data[i] == value => i
		}

		=> -1
	}

	# Summary: Returns the index of the first occurrence of the specified character
	index_of(value: char, start: large) {
		require(start >= 0 and start <= length, 'Invalid start index')
		
		a = length

		loop (i = start, i < a, i++) {
			if data[i] == value => i
		}

		=> -1
	}

	# Summary: Returns the index of the first occurrence of the specified string
	index_of(value: String) {
		=> index_of(value.data, value.length, 0)
	}

	# Summary: Returns the index of the first occurrence of the specified string
	index_of(value: link) {
		=> index_of(value, length_of(value), 0)
	}

	# Summary: Returns the index of the first occurrence of the specified string
	index_of(value: String, start: large) {
		require(start >= 0 and start <= length, 'Invalid start index')
		=> index_of(value.data, value.length, start)
	}

	# Summary: Returns the index of the first occurrence of the specified string
	index_of(value: link, start: large) {
		require(start >= 0 and start <= length, 'Invalid start index')
		=> index_of(value, length_of(value), start)
	}

	# Summary: Returns the index of the first occurrence of the specified string
	index_of(value: link, value_length: large, start: large) {
		length: large = this.length
		require(start >= 0 and start <= length, 'Invalid start index')

		loop (i = start, i <= length - value_length, i++) {
			match = true

			loop (j = 0, j < value_length, j++) {
				if data[i + j] == value[j] continue
				match = false
				stop
			}

			if match => i
		}

		=> -1
	}

	# Summary: Returns the index of the last occurrence of the specified character
	last_index_of(value: char) {
		loop (i = length - 1, i >= 0, i--) {
			if data[i] == value => i
		}

		=> -1
	}

	# Summary: Converts all upper case alphabetic characters to lower case and returns a new string
	to_lower() {
		buffer = allocate(length + 1)
		buffer[length] = 0

		loop (i = 0, i < length, i++) {
			value = data[i]
			if value >= `A` and value <= `Z` { value -= (`A` - `a`) }
			buffer[i] = value
		}

		=> String.from(buffer, length)
	}

	# Summary: Converts all lower case alphabetic characters to upper case and returns a new string
	to_upper() {
		buffer = allocate(length + 1)
		buffer[length] = 0

		loop (i = 0, i < length, i++) {
			value = data[i]
			if value >= `a` and value <= `z` { value += (`A` - `a`) }
			buffer[i] = value
		}

		=> String.from(buffer, length)
	}

	# Summary: Adds the two strings together and returns a new string
	plus(string: link, length: large) {
		a = this.length
		b = length
		c = a + b

		memory = allocate(c + 1) # Include the zero byte

		copy(data, a, memory)
		copy(string, b, memory + a)

		result = String()
		result.data = memory
		result.length = c
		=> result
	}

	# Summary: Adds the two strings together and returns a new string
	plus(string: String) {
		=> plus(string.data, string.length)
	}

	# Summary: Adds the two strings together and returns a new string
	plus(other: link) {
		=> plus(other, length_of(other))
	}

	# Summary: Creates a new string which has this string in the beginning and the specified character added to the end
	plus(other: char) {
		a = length

		# Allocate memory for new string
		memory = allocate(a + 2)

		# Copy this string to the new string
		copy(data, a, memory)
		
		# Add the given character to the end of the new string
		memory[a] = other
		memory[a + 1] = 0

		result = String()
		result.data = memory
		result.length = a + 1
		=> result
	}

	# Summary: Overrides the indexed accessor, returning the character in the specified position
	get(i: large) {
		require(i >= 0 and i <= length, 'Invalid getter index')
		=> data[i] as char
	}

	# Summary: Overrides the indexed accessor, allowing the user to edit the character in the specified position
	set(i: large, value: char) {
		require(i >= 0 and i <= length, 'Invalid setter index')
		data[i] = value
	}

	# Summary: Returns whether the two strings are equal
	equals(other: String) {
		a = length
		b = other.length

		if a != b => false

		loop (i = 0, i < a, i++) {
			if data[i] != other.data[i] => false
		}

		=> true
	}

	# Summary: Returns whether the two strings are equal
	equals(data: link) {
		a = length
		b = length_of(data)

		if a != b => false

		loop (i = 0, i < a, i++) {
			if this.data[i] != data[i] => false
		}

		=> true
	}

	# Summary: Computes hash code for the string
	hash() {
		hash = 5381
		a = length

		loop (i = 0, i < a, i++) {
			hash = ((hash <| 5) + hash) + data[i] # hash = hash * 33 + data[i]
		}

		=> hash
	}
}