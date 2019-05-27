using GanglionUnity.Internal;
using NUnit.Framework;

namespace Tests
{
    class OrderedChunkBufferTests
    {

        [Test]
        public void CheckEmptyState()
        {
            int CHUNK_SIZE = 32, CHUNK_N = 2;
            var buffer = new OrderedChunkBuffer<float>(CHUNK_SIZE, CHUNK_N);

            Assert.AreEqual(CHUNK_SIZE, buffer.ChunkSize);
            Assert.AreEqual(CHUNK_N, buffer.ChunksCount);
            Assert.AreEqual(CHUNK_SIZE * CHUNK_N, buffer.Length);
            Assert.AreEqual(false, buffer.IsFull);
        }
        
        [Test]
        public void CheckIsFullBounds()
        {
            int CHUNK_SIZE = 64, CHUNK_N = 3;
            var buffer = new OrderedChunkBuffer<float>(CHUNK_SIZE, CHUNK_N);

            for(int i = 0; i < buffer.ChunkSize - 1; i++)
                buffer.Write(1);
            Assert.AreEqual(false, buffer.IsFull);
            buffer.Write(1);
            Assert.AreEqual(true, buffer.IsFull);
            buffer.Write(1);
            Assert.AreEqual(false, buffer.IsFull);
        }

        [Test]
        public void CheckBracketsBounds()
        {
            int CHUNK_SIZE = 64, CHUNK_N = 2;
            var buffer = new OrderedChunkBuffer<float>(CHUNK_SIZE, CHUNK_N);
            for (int i = 1; i < buffer.ChunkSize + 1; i++)
                buffer.Write(i);

            Assert.AreEqual(1, buffer[CHUNK_N - 1, 0]);
            Assert.AreEqual(1, buffer[(CHUNK_N - 1) * CHUNK_SIZE]);

            Assert.AreEqual(CHUNK_SIZE, buffer[CHUNK_N - 1, CHUNK_SIZE - 1]);
            Assert.AreEqual(CHUNK_SIZE, buffer[CHUNK_N * CHUNK_SIZE - 1]);
        }

        [Test]
        public void CheckLeftShift()
        {
            int CHUNK_SIZE = 64, CHUNK_N = 3;
            var buffer = new OrderedChunkBuffer<float>(CHUNK_SIZE, CHUNK_N);
            for (int i = 0; i < buffer.ChunkSize; i++)
                buffer.Write(1);
            for (int i = 0; i < buffer.ChunkSize; i++)
                buffer.Write(2);
            Assert.AreEqual(1, buffer[CHUNK_N - 2, 0]);
            Assert.AreEqual(1, buffer[CHUNK_N - 2, CHUNK_SIZE - 1]);
            Assert.AreEqual(2, buffer[CHUNK_N - 1, 0]);
            Assert.AreEqual(2, buffer[CHUNK_N - 1, CHUNK_SIZE - 1]);

            for (int i = 0; i < buffer.ChunkSize; i++)
                buffer.Write(3);
            for (int i = 0; i < buffer.ChunkSize; i++)
                buffer.Write(4);
            for (int i = 0; i < CHUNK_N; i++)
            {
                Assert.AreEqual(2 + i, buffer[i, 0]);
                Assert.AreEqual(2 + i, buffer[i, CHUNK_SIZE - 1]);
            }
        }
    }
}
