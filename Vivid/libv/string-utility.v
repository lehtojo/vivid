STRING_DECIMAL_PRECISION = 15

# Summary: Returns the length of the specified string
export length_of(text: link) {
	length = 0

	loop {
		if text[length] == 0 => length
		length++
	}
}

# Summary: Returns the index of the first occurrence of the specified character in the specified string
export index_of(string: link, character: char) {
	length = length_of(string)

	loop (i = 0, i < length, i++) {
		if string[i] == character => i
	}

	=> -1
}

# Summary: Returns whether the specified character is a digit
export is_digit(value: char) {
	=> value >= `0` and value <= `9`
}

# Summary: Returns whether the specified character is an alphabet
export is_alphabet(value: char) {
	=> (value >= `a` and value <= `z`) or (value >= `A` and value <= `Z`)
}

# Summary: Converts the specified number into a string and stores it in the specified buffer
export to_string(number: large, result: link) {
	position = 0

	if number < 0 {
		loop {
			a = number / 10
			remainder = number - a * 10
			number = a

			result[position] = `0` - remainder
			position++

			if number == 0 stop
		}

		result[position] = `-`
		position++
	}
	else {
		loop {
			a = number / 10
			remainder = number - a * 10
			number = a

			result[position] = `0` + remainder
			position++

			if number == 0 stop
		}
	}

	reverse(result, position)
	=> position
}

# Summary: Converts the specified number into a string and stores it in the specified buffer
export to_string(number: decimal, result: link) {
	position = to_string(number as large, result)

	# Remove the integer part
	number -= number as large

	# Ensure the number is a positive number
	if number < 0 { number = -number }

	# Add the decimal point
	result[position] = `,`
	position++

	# If the number is zero, skip the fractional part computation
	if number == 0 {
		result[position] = `0`
		=> position + 1
	}

	# Compute the fractional part
	loop (i = 0, i < STRING_DECIMAL_PRECISION and number > 0, i++) {
		number *= 10
		digit = number as large
		number -= digit

		result[position] = `0` + digit
		position++
	}

	=> position
}

# Summary: Converts the specified string into an integer
export to_integer(string: link, length: large) {
	require(length >= 0, 'String can not be empty when converting to integer')

	result = 0
	index = 0
	sign = 1

	if string[0] == `-` {
		sign = -1
		index++
	}

	loop (index < length) {
		digit = (string[index] as large) - `0`
		result = result * 10 + digit
		index++
	}

	=> result * sign
}

# Summary: Converts the specified string to a decimal using the specified separator
export to_decimal(string: link, length: large, separator: char) {
	require(length >= 0, 'String can not be empty when converting to integer')

	# Find the index of the separator
	separator_index = -1

	loop (i = 0, i < length, i++) {
		if string[i] == separator {
			separator_index = i
			stop
		}
	}

	# If the separator does not exist, we can treat the string as an integer
	if separator_index < 0 => to_integer(string, length) as decimal

	# Compute the integer value before the separator
	integer_value = to_integer(string, separator_index) as decimal

	# Compute the index of the first digit after the separator where we start
	start = separator_index + 1
	
	# Set the precision to be equal to the number of digits after the separator by default
	precision = length - start

	# Limit the precision
	if precision > STRING_DECIMAL_PRECISION { precision = STRING_DECIMAL_PRECISION }

	# Compute the index of the digit after the last included digit of the fractional part
	end = start + precision

	fraction = 0
	scale = 1

	loop (i = start, i < end, i++) {
		fraction = fraction * 10 + (string[i] - `0`)
		scale *= 10
	}

	if integer_value < 0 => integer_value - fraction / (scale as decimal)
	=> integer_value + fraction / (scale as decimal)
}

# Summary: Converts the specified string to a decimal
export to_decimal(string: link, length: large) {
	=> to_decimal(string, length, `.`)
}

# Summary: Tries to convert the specified string to a decimal number
export as_decimal(string: link, length: large) {
	index = 0

	first = string[0]
	if first == `-` or first == `+` { index++ }

	separated = false

	loop (index < length, index++) {
		character = string[index]

		if is_digit(character) continue
		if separated or character != `.` => Optional<decimal>()

		separated = true
	}

	=> Optional<decimal>(to_decimal(string, length))
}

# Summary: Tries to convert the specified string to an integer number
export as_integer(string: link, length: large) {
	index = 0

	first = string[0]
	if first == `-` or first == `+` { index++ }

	loop (index < length, index++) {
		if not is_digit(string[index]) => Optional<large>()
	}

	=> Optional<large>(to_integer(string, length))
}