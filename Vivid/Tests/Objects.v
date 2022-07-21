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
	return Apple()
}

export create_car(price: decimal) {
	return Car(price)
}

init() {
	return 1
}