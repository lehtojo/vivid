TYPE_DESCRIPTOR_NAME_OFFSET = 0
TYPE_DESCRIPTOR_SIZE_OFFSET = 1
TYPE_DESCRIPTOR_SUPERTYPES_COUNT_OFFSET = 2
TYPE_DESCRIPTOR_SUPERTYPES_FIRST = 3

export TypeDescriptor {
	private address: link

	private get_supertype_count() {
		return address[TYPE_DESCRIPTOR_SUPERTYPES_COUNT_OFFSET] as normal
	}

	init(address: link) {
		this.address = (address as link<link<link<large>>>)[0][0]
	}

	name() {
		return String(address[TYPE_DESCRIPTOR_NAME_OFFSET] as link)
	}

	size() {
		return address[TYPE_DESCRIPTOR_SIZE_OFFSET] as normal
	}

	supertypes() {
		count = get_supertype_count()
		supertypes = List<TypeDescriptor>(count, false)

		loop (i = 0, i < count, i++) {
			supertype = address[TYPE_DESCRIPTOR_SUPERTYPES_FIRST + i] as link
			supertypes[i] = TypeDescriptor(supertype)
		}

		return supertypes
	}
}

export typeof(object) {
	return TypeDescriptor(object as link)
}