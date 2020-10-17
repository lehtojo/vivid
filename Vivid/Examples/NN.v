error = 0
ok = 1
none = 0

random() {
	=> 0.5
}

Array { T } {
   private data: T

   count: num

   init(c: num) {
	  count = c
	  data = T[count]
   }

   set(i: num, value: T) {
	  data[i] = value
   }

   at(i: num) {
	  => data[i]
   }
}

Node {
   value: decimal
   connections: Array(Connection)

   init() {
	  value = 0.0
   }

   connect(layer: Layer) {
	  connections = Array(Connection, layer.size)
		
	  i = 0
	  loop (i = 0, i < layer.size, ++i) {
		 connection = Connection(layer.at(i))
		 connections.set(i, connection)
	  }
   }

   signal() {
	  i = 0
	  loop (i = 0, i < connections.count, ++i) {
		 connections.at(i).send(value)
	  }
   }
}

Connection {
   private receiver: Node
   weight: decimal

   init(r: Node) {
	  receiver = r
	  weight = random()
   }

   send(value: decimal) {
	  receiver.value = receiver.value + value * weight
   }
}

Layer {
   size: num
   nodes: Array(Node)

   init(s: num) {
	  size = s
	  nodes = Array(Node, size)
   }

   connect(other: Layer) {
	  i = 0
	  loop (i = 0, i < size, ++i) {
		 nodes.at(i).connect(other)
	  }
   }

   signal() {
	  i = 0
	  loop (i = 0, i < nodes.count, ++i) {
		 nodes.at(i).signal()
	  }
   }

   at(i: num) {
	  => nodes.at(i)
   }
}

Network {
   layers: Array(Layer)

   init(inputs: num, hidden_layer_count: num, hidden_layer_size: num, outputs: num) {
	  layers = Array(Layer, hidden_layer_count + 2)

	  # Create the layers
	  layers.set(0, Layer(inputs))
		
	  i = 0
	  loop (i = 0, i < hidden_layer_count, ++i) {
		 layers.set(i + 1, Layer(hidden_layer_size))
	  }

	  layers.set(hidden_layer_count + 1, Layer(outputs))

	  loop (i = 0, i < layers.count - 1, ++i) {
		 # Connect the current layer to the next one
		 layers.at(i).connect(layers.at(i + 1))
	  }
   }

   process(input: Array(decimal)) {
	  if input.count != layers.count {
		 => error
	  }

	  first = layers.at(0)
	  i = 0

	  loop (i = 0, i < first.size, ++i) {
		 first.at(i).value = input.at(i)
	  }

	  loop (i = 0, i < layers.count, ++i) {
		 layers.at(i).signal()
	  }

	  => ok
   }
}

init() {
   network = Network(10, 16, 16, 4)

   input = Array(decimal, 6)
   input.set(0, 1.0)
   input.set(1, 0.0)
   input.set(2, 1.0)
   input.set(3, 0.0)
   input.set(4, 0.0)
   input.set(5, 1.0)

   network.process(input)
}

