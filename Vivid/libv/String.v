DECIMAL_PRECISION = 15

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

	loop (i = 0, i < DECIMAL_PRECISION and n > 0, i++) {
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
	length = text.length()

	if length == 0 {
		=> 0
	}

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

String {
	private text: link

	init(source: link) {
		text = source
	}

	###
	Summary: Creates a new string which has this string in the begining and the specified string added to the end
	###
	combine(other: String) {
		a = length()
		b = other.length() + 1

		memory = allocate(a + b)

		copy(text, a, memory)
		offset_copy(other.text, b, memory, a)

		=> String(memory)
	}

	###
	Summary: Creates a new string which has this string in the begining and the specified character added to the end
	###
	append(character: u8) {
		length = length()

		# Allocate memory for new string
		memory = allocate(length + 2)

		# Copy this string to the new string
		copy(text, length, memory)
		
		# Add the given character to the end of the new string
		memory[length] = character
		memory[length + 1] = 0

		=> String(memory)
	}

	insert(index: large, character: u8) {
		# Calculate the current string length
		length = length()

		# Reserve memory: Current memory + Character + Terminator
		memory = allocate(length + 2)

		# Copy the first segment before the index to the buffer
		copy(text, index, memory)
		# Copy the second segment after the index to the buffer, leaving space for the character
		offset_copy(text, length - index, memory, index + 1)

		# Insert the character and the terminator
		memory[index] = character
		memory[length + 1] = 0

		# Create a new string from the buffer
		=> String(memory)
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

	###
	Summary: Returns the length of this string
	###
	length() {
		i = 0
		
		loop(text[i] != 0) { ++i }

		=> i
	}

	equals(other: String) {
		a = length()
		b = other.length()

		if a != b => false as bool

		loop (i = 0, i < a, i++) {
			if text[i] != other.text[i] => false as bool
		}

		=> true as bool
	}

	equals(text: link) {
		a = length()
		b = length_of(text)

		if a != b => false as bool

		loop (i = 0, i < a, i++) {
			if this.text[i] != text[i] => false as bool
		}

		=> true as bool
	}

	hash() {
		hash = 1
		length = length()

		loop (i = 0, i < length, i++) {
			hash *= text[i] as large
		}

		=> hash
	}
}