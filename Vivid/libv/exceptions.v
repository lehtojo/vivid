export Outcome<T, E> {
	is_error: bool

	has_value() {
		return not is_error
	}

	get_value() {
		if is_error panic('Outcome has no value')
		return this.(Ok<T, E>).value
	}

	get_error() {
		if not is_error panic('Outcome has no error')
		return this.(Error<T, E>).error
	}

	# Summary: Returns the specified fallback value if the outcome represents an error, otherwise the contained value is returned
	value_or(fallback: T) {
		if is_error return fallback
		return this.(Ok<T, E>).value
	}
}

export Outcome<T, E> Ok<T, E> {
	value: T

	init(value: T) {
		this.value = value
		this.is_error = false
	}
}

export Outcome<T, E> Error<T, E> {
	error: E

	init(error: E) {
		this.error = error
		this.is_error = true
	}
}

export Optional<T> {
	value: T
	empty: bool

	init() {
		empty = true
	}

	init(value: T) {
		this.value = value
		this.empty = false
	}

	has_value() {
		return not empty
	}

	get_value() {
		return value
	}

	value_or(fallback: T) {
		result = value
		if empty { result = fallback }
		return result
	}
}

# Summary: Ensures the specified condition is true, otherwise this function exits the application and informs that the requirement was not met
export require(result: bool) {
	if not result panic('Requirement failed')
}

# Summary: Ensures the specified condition is true, otherwise this function exits the application and informs the user with the specified message
export require(result: bool, message: link) {
	if not result panic(message)
}

# Summary: Ensures the specified condition is true, otherwise this function exits the application and informs the user with the specified message
export require(result: bool, message: String) {
	require(result, message.data)
}

# Summary: Writes the specified message to the console and exits the application with code 1
export panic(message: link) {
	internal.console.write(message, length_of(message))
	application.exit(1)
}