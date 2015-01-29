using System.Collections.Generic;
using System.Linq;
using Craft.Net.Common;
using fNbt;
using MiNET.Utils;

namespace MiNET.Worlds
{
	public class FlatlandWorldProvider : IWorldProvider
	{
		private readonly List<ChunkColumn> _chunkCache = new List<ChunkColumn>();
		private bool _loadFromFile;
		private bool _saveToFile;

		public bool IsCaching { get; private set; }

		public FlatlandWorldProvider()
		{
			IsCaching = true;
#if DEBUG
			_loadFromFile = ConfigParser.GetProperty("load_pe", false);
			_saveToFile = ConfigParser.GetProperty("save_pe", false);
#else
			_loadFromFile = ConfigParser.GetProperty("load_pe", true);
			_saveToFile = ConfigParser.GetProperty("save_pe", true);
#endif
		}

		public void Initialize()
		{
		}

		public ChunkColumn GenerateChunkColumn(Coordinates2D chunkCoordinates)
		{
			lock (_chunkCache)
			{
				ChunkColumn cachedChunk = _chunkCache.FirstOrDefault(chunk2 => chunk2 != null && chunk2.x == chunkCoordinates.X && chunk2.z == chunkCoordinates.Z);

				if (cachedChunk != null)
				{
					return cachedChunk;
				}

				ChunkColumn chunk = new ChunkColumn();
				chunk.x = chunkCoordinates.X;
				chunk.z = chunkCoordinates.Z;

				bool loaded = false;
				if (_loadFromFile)
				{
					loaded = chunk.TryLoadFromFile();
				}

				if (!loaded)
				{
					PopulateChunk(chunk);

					chunk.SetBlock(0, 5, 0, 7);
					chunk.SetBlock(1, 5, 0, 41);
					chunk.SetBlock(2, 5, 0, 41);
					chunk.SetBlock(3, 5, 0, 41);
					chunk.SetBlock(3, 5, 0, 41);

					//chunk.SetBlock(6, 5, 6, 57);

					chunk.SetBlock(6, 4, 9, 63);
					chunk.SetMetadata(6, 4, 9, 12);
					chunk.BlockEntity = GetBlockEntity((chunkCoordinates.X*16) + 6, 4, (chunkCoordinates.Z*16) + 9);

					if (chunkCoordinates.X == 1 && chunkCoordinates.Z == 1)
					{
						for (int x = 0; x < 16; x++)
						{
							for (int z = 0; z < 16; z++)
							{
								for (int y = 2; y < 4; y++)
								{
									chunk.SetBlock(x, y, z, 8);
								}
							}
						}
					}

					if (chunkCoordinates.X == 3 && chunkCoordinates.Z == 1)
					{
						for (int x = 0; x < 16; x++)
						{
							for (int z = 0; z < 16; z++)
							{
								for (int y = 3; y < 4; y++)
								{
									chunk.SetBlock(x, y, z, 10);
								}
							}
						}
					}
				}
				_chunkCache.Add(chunk);

				return chunk;
			}
		}

		public Coordinates3D GetSpawnPoint()
		{
			return new Coordinates3D(50, 10, 50);
		}

		public void PopulateChunk(ChunkColumn chunk)
		{
			var random = new CryptoRandom();
			var stones = new byte[16*16*128];

			for (int i = 0; i < stones.Length; i = i + 128)
			{
				stones[i] = 7; // Bedrock
				int h = 1;

				stones[i + h++] = 1; // Stone
				stones[i + h++] = 1; // Stone

				switch (random.Next(0, 20))
				{
					case 0:
						stones[i + h++] = 3; // Dirt
						stones[i + h++] = 3;
						break;
					case 1:
						stones[i + h++] = 1; // Stone
						stones[i + h++] = 1; // Stone
						break;
					case 2:
						stones[i + h++] = 13; // Gravel
						stones[i + h++] = 13; // Gravel
						break;
					case 3:
						stones[i + h++] = 14; // Gold
						stones[i + h++] = 14; // Gold
						break;
					case 4:
						stones[i + h++] = 16; // Cole
						stones[i + h++] = 16; // Cole
						break;
					case 5:
						stones[i + h++] = 56; // Dimond
						stones[i + h++] = 56; // Dimond
						break;
					default:
						stones[i + h++] = 1; // Stone
						stones[i + h++] = 1; // Stone
						break;
				}
				stones[i + h++] = 3; // Dirt
				stones[i + h++] = 2; // Grass
			}

			chunk.blocks = stones;
			//chunk.biomeColor = ArrayOf<int>.Create(256, random.Next(6761930, 8761930));
//			for (int i = 0; i < chunk.biomeColor.Length; i++)
//			{
//				chunk.biomeColor[i] = random.Next(6761930, 8761930);
//			}
		}

		public void SaveChunks()
		{
			if (_saveToFile)
			{
				foreach (ChunkColumn chunkColumn in _chunkCache)
				{
					chunkColumn.SaveChunk();
				}
			}
		}

		private NbtFile GetBlockEntity(int x, int y, int z)
		{
			NbtFile file = new NbtFile();
			file.BigEndian = false;
			var compound = new NbtCompound(string.Empty);
			compound.Add(new NbtString("id", "Sign"));
			compound.Add(new NbtString("Text1", "first"));
			compound.Add(new NbtString("Text2", "second"));
			compound.Add(new NbtString("Text3", "third"));
			compound.Add(new NbtString("Text4", "forth"));
			compound.Add(new NbtInt("x", x));
			compound.Add(new NbtInt("y", y));
			compound.Add(new NbtInt("z", z));
			file.RootTag = compound;

			return file;
		}
	}
}