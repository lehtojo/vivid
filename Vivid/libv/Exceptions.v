export Outcome<T, E> {
	is_error: bool

	has_value() {
		=> !is_error
	}

	get_value() {
		if is_error panic('Outcome has no value')
		=> this.(Ok<T, E>).value
	}

	get_error() {
		if not is_error panic('Outcome has no error')
		=> this.(Error<T, E>).error
	}

	# Summary: Returns the specified fallback value if the outcome represents an error, otherwise the contained value is returned
	value_or(fallback: T) {
		if is_error => fallback
		=> this.(Ok<T, E>).value
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
		=> !empty
	}

	get_value() {
		=> value
	}

	value_or(fallback: T) {
		result = value
		if empty { result = fallback }
		=> result
	}
}

export panic(message: link) {
	internal.console.write(message, length_of(message))
	application.exit(1)
}