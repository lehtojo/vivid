Result<T, E> Ok<T, E> {
	value: T
	init(value: T) { this.value = value }
}

Result<T, E> Error<T, E> {
	error: E
	init(error: E) { this.error = error }
}

Result<T, E> {
	shared from(value: T): Result<T, E> {
		return Ok<T, E>(value)
	}

	shared from(error: E): Result<T, E> {
		return Error<T, E>(error)
	}

	has_value => this is Ok<T, E>
	has_error => this is Error<T, E>

	get_value => this.(Ok<T, E>).value
	get_error => this.(Error<T, E>).error
}

StringView {
	data: u8*

	shared from(data: u8*): StringView {
		return StringView(data)
	}

	init(data: u8*) {
		this.data = data
	}
}

export test_1(a: i64, b: i64): Result<i64, u8*> {
	if a > b {
		return a + b
	}

	return 'a is not greater than b'
}

export test_2(): StringView {
	view: StringView = 'Hello there :^)'
	return view
}

init() {
	are_equal(test_1(42, 7) has sum, true)
	are_equal(sum, 49)
	result = test_1(-42, -7)
	are_equal(result.has_error, true)
	are_equal(String(result.get_error()), "a is not greater than b")
	are_equal(String(test_2().data), "Hello there :^)")
	return 0
}