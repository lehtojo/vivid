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

init() {
	=> 1
	
	# Dummy for type resolvation
	t = Holder()
	assignment(t)
}