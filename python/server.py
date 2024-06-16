import socketio

# Initialize Socket.IO server
sio = socketio.Server(cors_allowed_origins='*')
app = socketio.WSGIApp(sio)

# Define event handlers
@sio.event
def connect(sid, environ):
    print('Connected:', sid)

@sio.event
def disconnect(sid):
    print('Disconnected:', sid)

@sio.event
def message(sid, data):
    print('Message received:', data)
    # Handle incoming message as needed
    # You can broadcast this message to other clients if required
    sio.emit('response', data)

# Run the server
if __name__ == '__main__':
    import eventlet
    eventlet.wsgi.server(eventlet.listen(('localhost', 5000)), app)
