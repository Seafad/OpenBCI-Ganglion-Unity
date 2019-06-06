using System;

namespace GanglionUnity.Internal
{
    /// <summary>
    /// Stores data in an ordered set of chunks
    /// </summary>
    public class OrderedChunkBuffer<T>
    {
        /// <summary>
        /// Access an element by its' buffer index/>
        /// </summary>
        /// <returns><paramref name="index"/>th element in the buffer</returns>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index > buffer.Length) throw new ArgumentOutOfRangeException("index");
                return buffer[index];
            }
            private set { }
        }
        /// <summary>
        /// Access an element by its' chunk index
        /// </summary>
        /// <returns>Returns <paramref name="valueIndex"/>th element of <paramref name="chunkIndex"/>th chunk</returns>
        public T this[int chunkIndex, int valueIndex]
        {
            get
            {
                if (chunkIndex < 0 || chunkIndex >= ChunksCount) throw new ArgumentOutOfRangeException("chunkIndex");
                if (valueIndex < 0 || valueIndex >= ChunkSize) throw new ArgumentOutOfRangeException("valueIndex");
                return buffer[chunkIndex * ChunkSize + valueIndex];
            }
            private set { }
        }
        public int ChunkSize { get; }
        public int ChunksCount { get; }
        public int Length { get { return buffer.Length; } }
        public bool IsFull { get; private set; }

        private T[] buffer;
        private int lastChunkStartIndex, bufferWriteIndex;

        public OrderedChunkBuffer() : this(64, 4) { }

        public OrderedChunkBuffer(int chunkSize, int chunksCount)
        {
            this.ChunkSize = chunkSize;
            this.ChunksCount = chunksCount;
            buffer = new T[chunkSize * chunksCount];
            lastChunkStartIndex = chunkSize * (chunksCount - 1);
            bufferWriteIndex = lastChunkStartIndex;
        }

        public void Write(T value)
        {
            if (IsFull)
            {
                ShiftLeft(1);
                IsFull = false;
            }
            buffer[bufferWriteIndex++] = value;
            if (bufferWriteIndex == buffer.Length)
                IsFull = true;
        }

        private void ShiftLeft(int chunksNum)
        {
            Array.Copy(buffer, ChunkSize * chunksNum, buffer, 0, lastChunkStartIndex);
            Array.Clear(buffer, lastChunkStartIndex, ChunkSize);
            bufferWriteIndex = lastChunkStartIndex;
        }

        public T[] GetAllValues()
        {
            return buffer;
        }

        public void Clear()
        {
            Array.Clear(buffer, 0, buffer.Length);
            bufferWriteIndex = lastChunkStartIndex;
        }

        public float[] GetLastChunkValues()
        {
            float[] lastChunkVals = new float[ChunkSize];
            Array.Copy(buffer, ChunkSize * (ChunksCount - 1), lastChunkVals, 0, ChunkSize);
            return lastChunkVals;
        }

        public void SetChunkValues(float[] values, int chunkIndex)
        {
            if(chunkIndex < 0 || chunkIndex >= ChunksCount) throw new ArgumentOutOfRangeException("chunkIndex");
            if(values.Length != ChunkSize) throw new ArgumentException("Array size should be equal to ChunkSize", "values");
            Array.Copy(values, 0, buffer, ChunkSize * chunkIndex, ChunkSize);
        }
  
    }
}
