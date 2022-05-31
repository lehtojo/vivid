TYPE_DESCRIPTOR_NAME_OFFSET = 0
TYPE_DESCRIPTOR_SIZE_OFFSET = 1
TYPE_DESCRIPTOR_SUPERTYPES_COUNT_OFFSET = 2
TYPE_DESCRIPTOR_SUPERTYPES_FIRST = 3

export TypeDescriptor {
	private address: link

	private get_supertype_count() {
		=> address[TYPE_DESCRIPTOR_SUPERTYPES_COUNT_OFFSET] as normal
	}

	init(address: link) {
		this.address = (address as link<link<link<large>>>)[0][0]
	}

	name() {
		=> String(address[TYPE_DESCRIPTOR_NAME_OFFSET] as link)
	}

	size() {
		=> address[TYPE_DESCRIPTOR_SIZE_OFFSET] as normal
	}

	supertypes() {
		count = get_supertype_count()
		supertypes = List<TypeDescriptor>(count, false)

		loop (i = 0, i < count, i++) {
			supertype = address[TYPE_DESCRIPTOR_SUPERTYPES_FIRST + i] as link
			supertypes[i] = TypeDescriptor(supertype)
		}

		=> supertypes
	}
}

export typeof(object) {
	=> TypeDescriptor(object as link)
}