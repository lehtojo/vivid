Holder {
	Normal: normal
	Tiny: tiny
	Double: decimal
	Large: large
	Small: small
}

Sequence {
	address: link<decimal>
}

# Tests whether the compiler can store values into object instances
export assignment_1(instance: Holder) {
	instance.Normal = 314159265
	instance.Tiny = 64
	instance.Double = 1.414
	instance.Large = -2718281828459045
	instance.Small = 12345
}

# Tests whether the compiler can store values into raw memory
export assignment_2(instance: Sequence) {
	instance.address[0] = -123.456
	instance.address[1] = -987.654
	instance.address[2] = 101.010
}

init() => true