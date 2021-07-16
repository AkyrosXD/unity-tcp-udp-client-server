CC = gcc
CFLAGS = -Wall
LDFLAGS = -pthread
OBJFILES = unity.o server.o main.o
TARGET = server
all: $(TARGET)
$(TARGET): $(OBJFILES)
	$(CC) $(CFLAGS) -o $(TARGET) $(OBJFILES) $(LDFLAGS)
clean:
	rm -f $(OBJFILES) $(TARGET) *~