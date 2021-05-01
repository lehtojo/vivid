Holder {
	Normal: normal
	Tiny: tiny
	Double: decimal
	Large: large
	Small: small
}

Sequence {
	address: link<decimal>

	# Takes the current values from the memory address and moves to the next element
	pop() {
		value = address[0]
		address += sizeof(decimal)
		=> value
	}
	
	# Moves to the next element
	next() {
		address += sizeof(decimal)
		=> 0
	}
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

# Tests whether the compiler loads the destination before executing the function call which modifies the address
export preload_1(instance: Sequence, i: large) {
	instance.address[i] = instance.pop() + 0.5
}

# Tests whether the compiler loads the left operand completely before executing the function call which modifies the address
export preload_2(instance: Sequence) {
	=> instance.address + instance.next()
}

# Tests whether the compiler loads the destination before incrementing it
export preload_3(address: link<large>) {
	address[0] = ++address as large
	=> address
}

init() => true