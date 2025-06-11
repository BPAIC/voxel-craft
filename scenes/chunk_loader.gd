class_name ChunkLoader extends Resource

var thread = Thread.new()
var is_running = true
var sem = Semaphore.new()

var chunk_positions_to_load = []
var chunk_positions_to_load_mtx = Mutex.new()

var loaded_chunks = []
var loaded_chunks_mtx = Mutex.new()

var chunk_positions_to_load_local = []
var loaded_chunks_local = []

func _init() -> void:
	thread.start(loop)

func _exit_tree() -> void:
	is_running = false
	sem.post()
	thread.wait_to_finish()

func push_chunk_positions_to_load(new_chunk_positions_to_load: Array):
	loaded_chunks_mtx.lock()
	chunk_positions_to_load_mtx.lock()
	
	# Check every chunk position in already prepared and loaded chunks
	for chunk_position in chunk_positions_to_load_local: # Currently processing queue
		if new_chunk_positions_to_load.has(chunk_position):
			new_chunk_positions_to_load.erase(chunk_position)
	for chunk_position in chunk_positions_to_load: # Queue to process
		if new_chunk_positions_to_load.has(chunk_position):
			new_chunk_positions_to_load.erase(chunk_position)
	for chunk in loaded_chunks: # Loaded chunks
		if new_chunk_positions_to_load.has(chunk.chunk_position):
			new_chunk_positions_to_load.erase(chunk.chunk_position)
			
	chunk_positions_to_load.append_array(new_chunk_positions_to_load)
	
	chunk_positions_to_load_mtx.unlock()
	loaded_chunks_mtx.unlock()
	sem.post()
	
func has_loaded_chunks() -> bool:
	var result = !loaded_chunks.is_empty()
	loaded_chunks_mtx.lock()
	loaded_chunks_mtx.unlock()
	return result
	
func get_loaded_chunks() -> Array:
	loaded_chunks_mtx.lock()
	var tmp = loaded_chunks.duplicate()
	loaded_chunks.clear()
	loaded_chunks_mtx.unlock()
	return tmp
	
func loop() -> void:	
	while is_running:
		sem.wait()

		# Copy "chunk positions to load" to local version to minimize mutex block
		chunk_positions_to_load_mtx.lock()
		chunk_positions_to_load_local = chunk_positions_to_load.duplicate()
		chunk_positions_to_load.clear()
		chunk_positions_to_load_mtx.unlock()
		
		# Loading
		for chunk_position_to_load in chunk_positions_to_load_local:
			var chunk = Chunk.new()
			chunk.chunk_position = chunk_position_to_load
			chunk.init()
			loaded_chunks_local.append(chunk)
			
		# Copy loaded chunks into output queue
		loaded_chunks_mtx.lock()
		loaded_chunks.append_array(loaded_chunks_local)
		loaded_chunks_mtx.unlock()
		loaded_chunks_local.clear()
