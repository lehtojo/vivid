Apple {
	weight = 100
	price = 0.1
}

Car {
	weight = 2000000
	brand = String('Flash')
	price: decimal

	init(p: decimal) {
		price = p
	}
}

export create_apple() {
	=> Apple()
}

export create_car(price: decimal) {
	=> Car(price)
}

init() {
	=> 1
}