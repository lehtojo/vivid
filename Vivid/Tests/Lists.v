# Test: Create a simple list of integers
list_1() {
	return [ 1, 2, 3, 5, 7, 11, 13 ]
}

sum(a, b) {
	return a + b
}

# Test: Create a list of integers, whose elements are expressions
list_2() {
	return [ 2 * (7 + 7) + 14, sum(6 * 6, 11 * 3) ]
}

Item {
	name: link

	init(name: link) {
		this.name = name
	}

	open string() {
		return String(name)
	}
}

Item Bundle {
	quantity: large

	init(name: link, quantity: large) {
		Item.init(name)
		this.quantity = quantity
	}

	override string() {
		return String(name) + ' x ' + to_string(quantity)
	}
}

# Test: Create a list of items (all items have the same type)
list_3() {
	return [
		Item('Foo'),
		Item('Bar'),
		Item('Baz'),
		Item('Qux'),
		Item('Xyzzy')
	]
}

# Test: Create a list of items (shared type)
list_4() {
	return [
		Item('Foo'), Item('Bar'), # Test: Multiline list can still add multiple items in one row
		Bundle('Baz', 3),
		Item('Qux'),
		Bundle('Xyzzy', 7), # The comma here is intentional, because the list should not care about it
	]
}

print_list(list) {
	loop element in list {
		if compiles { element.string() } {
			console.write(element.string())
		}
		else {
			console.write(element)
		}

		console.write(', ')
	}

	console.write_line()
}

init() {
	print_list(list_1())
	print_list(list_2())
	print_list(list_3())
	print_list(list_4())
	return 0
}