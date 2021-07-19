STRING_DECIMAL_PRECISION = 15
STRING_MINIMUM_DECIMAL = 0.000000000000001 # 0.1 ^ STRING_DECIMAL_PRECISION

###
Summary: Converts the specified integer number into a string
###
export to_string(n: large) {
	number = String('')
	sign = String('')

	if n < 0 {
		sign = String('-')
		n = -n
	}

	loop {
		remainder = n % 10
		n = n / 10

		number = number.insert(0, `0` + remainder)

		if n == 0 {
			=> sign.combine(number)
		}
	}
}

###
Summary: Converts the specified decimal number into a string
###
export to_string(n: decimal) {
	result = to_string(n as large)

	if n < 0 {
		n = -n
	}

	# Remove the integer part
	n -= n as large

	if n == 0 => result.combine(String(',0'))

	# Append comma
	result = result.append(`,`)

	loop (i = 0, i < STRING_DECIMAL_PRECISION and n > 0, i++) {
		n *= 10.0
		d = n as large
		n -= d

		result = result.append(`0` + d)
	}

	=> result
}

###
Summary: Converts the specified string into an integer number
###
export to_number(text: String) {
	length = text.length
	if length == 0 => 0

	buffer = text.data() as link
	sign = 1

	if buffer[0] == `-` {
		sign = -sign
	}

	i = 0
	n = 0

	loop (i < length) {
		a = buffer[i] as large - `0`
		n = n * 10 + a
		++i
	}

	=> n * sign
}

# Summary: Converts the specified string to a decimal number
export to_decimal(text: String) {
	i = text.index_of(`.`)
	if i == -1 => to_number(text) as decimal

	a = text.slice(0, i)
	b = text.slice(i + 1, text.length)

	value = to_number(text) as decimal
	k = STRING_MINIMUM_DECIMAL

	loop (i = STRING_DECIMAL_PRECISION - 1, i >= 0, i--) {
		if i >= b.length continue
		value += k * (b[i] - `0`)
		k *= 10
	}

	=> value
}

# Summary: Tries to convert the specified string to a decimal number
export as_decimal(text: String) {
	i = 0

	buffer = text.data()
	length = text.length
	first = buffer[0]

	if first == `-` or first == `+` { i++ }

	separated = false

	loop (i < length, i++) {
		a = buffer[i]

		if is_digit(a) continue
		if separated or a != `.` => Optional<decimal>()

		separated = true
	}

	=> Optional<decimal>(to_decimal(text))
}

# Summary: Tries to convert the specified string to an integer number
export as_number(text: String) {
	i = 0

	buffer = text.data()
	length = text.length
	first = buffer[0]

	if first == `-` or first == `+` { i++ }

	loop (i < length, i++) {
		if not is_digit(buffer[i]) => Optional<large>()
	}

	=> Optional<large>(to_number(text))
}

###
Summary: Returns the length of the specified string
###
export length_of(text: link) {
	i = 0

	loop {
		if text[i] == 0 => i
		++i
	}
}

# Summary: Returns whether the specified character is a digit
export is_digit(value: char) {
	=> value >= `0` and value <= `9`
}

# Summary: Returns whether the specified character is a digit
export is_alphabet(value: char) {
	=> (value >= `a` and value <= `z`) or (value >= `A` and value <= `Z`)
}

String {
	static empty: String

	# Summary: Combines all the specified strings while separating them the specified separator
	static join(separator: char, strings: List<String>) {
		if strings.size == 0 => String.empty

		result = strings[0]

		loop (i = 1, i < strings.size, i++) {
			result = result + separator + strings[i]
		}

		=> result
	}

	# Summary: Combines all the specified strings while separating them the specified separator
	static join(separator: String, strings: List<String>) {
		if strings.size == 0 => String.empty

		result = strings[0]

		loop (i = 1, i < strings.size, i++) {
			result = result + separator + strings[i]
		}

		=> result
	}

	public readonly text: link
	public readonly length: large

	# Summary: Returns whether this string is empty
	empty => length == 0

	# Summary: Creates a string from the specified data. Does not copy the content of the specified data.
	static from(text: link, length: large) {
		result = String()
		result.text = text
		result.length = length
		=> result
	}

	# Summary: Converts the specified character into a string
	init(value: char) {
		text = allocate(2)
		text[0] = value
		text[1] = 0
		length = 1
	}

	# Summary: Creates a string by copying the characters from the specified source
	init(source: link) {
		a = length_of(source)
		length = a

		text = allocate(a + 1)
		text[a] = 0

		copy(source, a, text)
	}

	# Summary: Creates a string by copying the characters from the specified source using the specified length
	init(source: link, length: large) {
		this.length = length

		text = allocate(length + 1)
		text[length] = 0

		copy(source, length, text)
	}

	# Summary: Creates an empty string
	private init() {}

	###
	Summary: Creates a new string which has this string in the begining and the specified string added to the end
	###
	combine(other: String) {
		a = length
		b = other.length + 1

		memory = allocate(a + b)

		copy(text, a, memory)
		offset_copy(other.text, b, memory, a)

		=> String(memory)
	}

	###
	Summary: Creates a new string which has this string in the begining and the specified character added to the end
	###
	append(character: u8) {
		a = length

		# Allocate memory for new string
		memory = allocate(a + 2)

		# Copy this string to the new string
		copy(text, a, memory)
		
		# Add the given character to the end of the new string
		memory[a] = character
		memory[a + 1] = 0

		=> String(memory)
	}

	insert(index: large, character: u8) {
		a = length

		# Reserve memory: Current memory + Character + Terminator
		memory = allocate(a + 2)

		# Copy the first segment before the index to the buffer
		copy(text, index, memory)
		# Copy the second segment after the index to the buffer, leaving space for the character
		offset_copy(text, a - index, memory, index + 1)

		# Insert the character and the terminator
		memory[index] = character
		memory[a + 1] = 0

		# Create a new string from the buffer
		=> String(memory)
	}

	# Summary: Returns whether the first characters match the specified string
	starts_with(start: String) {
		=> starts_with(start.text)
	}

	# Summary: Returns whether the first characters match the specified string
	starts_with(start: link) {
		a = length_of(start)
		if a == 0 or a > length => false

		loop (i = 0, i < a, i++) {
			if text[i] != start[i] => false
		}

		=> true
	}

	# Summary: Returns whether the first character matches the specified character
	starts_with(value: char) {
		=> length > 0 and text[0] == value
	}

	# Summary: Returns whether the last character matches the specified character
	ends_with(value: char) {
		=> length > 0 and text[length - 1] == value
	}

	# Summary: Returns whether the last characters match the specified string
	ends_with(end: link) {
		a = length_of(end)
		b = length

		if a == 0 or a > b => false

		loop (a > 0) {
			if end[--a] != text[--b] => false
		}

		=> true
	}

	# Summary: Returns the characters between the specified start and end index as a string
	slice(start: large, end: large) {
		a = length
		require(start >= 0 and start <= a and end >= start and end <= a)

		=> String(text + start, end - start)
	}

	# Summary: Replaces all the occurances of the specified character with the specified replacement
	replace(old: char, new: char) {
		a = length
		
		result = String(text, a)
		data = result.text

		loop (i = 0, i < a, i++) {
			if data[i] != old continue
			data[i] = new
		}

		=> result
	}

	# Summary: Returns the index of the first occurance of the specified character
	index_of(value: char) {
		a = length

		loop (i = 0, i < a, i++) {
			if text[i] == value => i
		}

		=> -1
	}

	# Summary: Returns the index of the first occurance of the specified character
	index_of(value: char, start: large) {
		if start < 0 => -1
		
		a = length

		loop (i = start, i < a, i++) {
			if text[i] == value => i
		}

		=> -1
	}

	# Summary: Returns the index of the last occurance of the specified character
	last_index_of(value: char) {
		loop (i = length - 1, i >= 0, i--) {
			if text[i] == value => i
		}

		=> -1
	}

	# Summary: Converts all upper case alphabetic characters to lower case and returns a new string
	to_lower() {
		buffer = allocate(length + 1)
		buffer[length] = 0

		loop (i = 0, i < length, i++) {
			value = text[i]
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
			value = text[i]
			if value >= `a` and value <= `z` { value += (`A` - `a`) }
			buffer[i] = value
		}

		=> String.from(buffer, length)
	}

	###
	Summary: Overrides the plus operator, allowing the user to combine string using the plus operator
	###
	plus(other: String) {
		=> combine(other)
	}

	###
	Summary: Overrides the plus operator, allowing the user to combine string using the plus operator
	###
	plus(other: link) {
		=> combine(String(other))
	}

	###
	Summary: Overrides the plus operator, allowing the user to combine character using the plus operator
	###
	plus(other: char) {
		=> append(other)
	}

	###
	Summary: Overrides the indexed accessor, returning the character in the specified position
	###
	get(i: large) {
		=> text[i] as u8
	}

	###
	Summary: Overrides the indexed accessor, allowing the user to edit the character in the specified position
	###
	set(i: large, value: u8) {
		text[i] = value
	}
	
	data() => text

	equals(other: String) {
		a = length
		b = other.length

		if a != b => false

		loop (i = 0, i < a, i++) {
			if text[i] != other.text[i] => false
		}

		=> true
	}

	equals(text: link) {
		a = length
		b = length_of(text)

		if a != b => false

		loop (i = 0, i < a, i++) {
			if this.text[i] != text[i] => false
		}

		=> true
	}

	hash() {
		hash = 1
		a = length

		loop (i = 0, i < a, i++) {
			hash *= text[i] as large
		}

		=> hash
	}
}