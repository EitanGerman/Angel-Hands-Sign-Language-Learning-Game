import socket
import threading
import time

class Server:
    def __init__(self):
        self.variable = "Initial Value"
        self.clients = []
        self.running = True

    def start(self):
        server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        server_socket.bind(('localhost', 65432))
        server_socket.listen(5)
        print("Server started, waiting for connections...")
        
        accept_thread = threading.Thread(target=self.accept_connections, daemon=True)
        accept_thread.start()
        
        update_thread = threading.Thread(target=self.update_variable, daemon=True)
        update_thread.start()

    def accept_connections(self):
        while self.running:
            client_socket, addr = server_socket.accept()
            print(f"Accepted connection from {addr}")
            self.clients.append(client_socket)

    def update_variable(self):
        while self.running:
            # Simulate generating a new value (replace with your own logic)
            new_value = self.generate_new_value()
            
            # Update the variable
            self.variable = new_value
            
            # Broadcast the new value to all connected clients
            self.broadcast(new_value)
            
            time.sleep(1)  # Adjust this delay as needed

    def generate_new_value(self):
        # Simulated function to generate a new value
        return f"Updated Value at {time.strftime('%H:%M:%S')}"

    def broadcast(self, message):
        for client_socket in self.clients:
            try:
                client_socket.send(message.encode())
            except Exception as e:
                print(f"Error broadcasting to client: {e}")
                self.clients.remove(client_socket)
                client_socket.close()

    def stop(self):
        self.running = False
        for client_socket in self.clients:
            client_socket.close()

if __name__ == "__main__":
    server = Server()
    server.start()

    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        print("\nStopping server...")
        server.stop()