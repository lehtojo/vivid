error = 0
ok = 1

Array { T } {
   private data: T[]

   count: num

   init(count: num) {
	  this.count = count
	  data = T[count]
   }

   get(i) => data[i]
   set(i, value) => data[i] = value
}

Node {
   value: decimal = 0.0
   connections: Connection Array

   connect(layer: Layer) {
	  connections = Connection Array(layer.size)

	  loop (i = 0, i < layer.size, ++i) {
		 connections.set(i, Connection(layer[i]))
	  }
   }

   signal() {
	  loop connection in connections {
		 connection.send(value)
	  }
   }
}

Connection {
   private receiver: Node
   open weight: dec

   init(receiver: Node) {
	  this.receiver = receiver
	  weight = random()
   }

   send(value: decimal) {
	  receiver.value += value * weight
   }
}

Layer {
   size: num
   nodes: Node Array

   init(size: num) {
	  this.size = size
	  nodes = Node Array(size)
   }

   connect(other: Layer) {
	  loop node in nodes {
		 node.connect(other)
	  }
   }

   signal() {
	  loop node in nodes {
		 node.signal()
	  }
   }

   get(i) => nodes[i]
   set(i, value) => nodes[i] = value
}

Network {
   layers: Layer Array

   init(inputs: num, hidden_layer_count: num, hidden_layer_size: num, outputs: num) {
	  layers = Layer Array(hidden_layer_count + 2)

	  # Create the layers
	  layers[0] = Layer(inputs)

	  loop (i = 0, i < hidden_layer_count, ++i) {
		 layers[i + 1] = Layer(hidden_layer_size)
	  }

	  layers[hidden_layer_count + 1] = Layer(outputs)

	  loop (i = 0, i < layers.size - 1, ++i) {
		 # Connect the current layer to the next one
		 layers[i].connect(layers[i + 1])
	  }
   }

   process(input: dec Array) {
	  if input.count != layers.count => error

	  first = layers.first

	  loop (i = 0, i < first.size, ++i) {
		 first[i].value = input[i]
	  }

	  loop layer in layers {
		 layer.signal()
	  }

	  => ok
   }
}

init() {
   network = Network(10, 16, 16, 4)
   network.process({ 1, 0, 1, 0, 0, 1 })
}

