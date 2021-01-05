Holder {
	Normal: normal
	Tiny: tiny
	Double: decimal
	Large: large
	Small: small
}

export assignment(target: Holder) {
	target.Normal = 314159265
	target.Tiny = 64
	target.Double = 1.414
	target.Large = -2718281828459045
	target.Small = 12345
}

MemoryAddressHolder {
	address: link<decimal>

	pop() {
		value = address[0]
		address += sizeof(decimal)
		=> value
	}
}

export memory_address_assignment(holder: MemoryAddressHolder) {
	holder.address[1] = holder.pop() + 0.5
}

init() => true