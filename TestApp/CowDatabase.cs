using Furesoft.Core.Storage;
using Furesoft.Core.Storage.Index;
using Furesoft.Core.Storage.Serializers;
using System;
using System.Collections.Generic;
using System.IO;

namespace TestApp
{
    internal class CowDatabase : IDisposable
    {
        readonly Stream mainDatabaseFile;
        readonly Stream primaryIndexFile;
        readonly Stream secondaryIndexFile;
        readonly Tree<Guid, uint> primaryIndex;
        readonly Tree<Tuple<string, int>, uint> secondaryIndex;
        private readonly RecordStorage cowRecords;
        private readonly CowSerializer cowSerializer = new CowSerializer();

        public CowDatabase(string pathToCowDb)
        {
            if (pathToCowDb == null)
                throw new ArgumentNullException("pathToCowDb");

            // As soon as CowDatabase is constructed, open the steam to talk to the underlying files
            mainDatabaseFile = new FileStream(pathToCowDb, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096);
            primaryIndexFile = new FileStream(pathToCowDb + ".pidx", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096);
            secondaryIndexFile = new FileStream(pathToCowDb + ".sidx", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096);

            // Construct the RecordStorage that use to store main cow data
            cowRecords = new RecordStorage(new BlockStorage(mainDatabaseFile, 4096, 48));

            // Construct the primary and secondary indexes
            primaryIndex = new Tree<Guid, uint>(
                new TreeDiskNodeManager<Guid, uint>(
                    new GuidSerializer(),
                    new TreeUIntSerializer(),
                    new RecordStorage(new BlockStorage(primaryIndexFile, 4096))
                ),
                false
            );

            secondaryIndex = new Tree<Tuple<string, int>, uint>(
                new TreeDiskNodeManager<Tuple<string, int>, uint>(
                    new StringIntSerializer(),
                    new TreeUIntSerializer(),
                    new RecordStorage(new BlockStorage(secondaryIndexFile, 4096))
                ),
                true
            );
        }

        /// <summary>
        /// Update given cow
        /// </summary>
        public void Update(CowModel cow)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("CowDatabase");
            }

            cowRecords.Update(primaryIndex.Get(cow.Id).Item2, cowSerializer.Serialize(cow));
        }

        /// <summary>
        /// Insert a new cow entry into our cow database
        /// </summary>
        public void Insert(CowModel cow)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("CowDatabase");
            }

            // Serialize the cow and insert it
            var recordId = cowRecords.Create(cowSerializer.Serialize(cow));

            // Primary index
            primaryIndex.Insert(cow.Id, recordId);

            // Secondary index
            secondaryIndex.Insert(new Tuple<string, int>(cow.Breed, cow.Age), recordId);
        }

        /// <summary>
        /// Find a cow by its unique id
        /// </summary>
        public CowModel Find(Guid cowId)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("CowDatabase");
            }

            // Look in the primary index for this cow
            var entry = primaryIndex.Get(cowId);
            if (entry == null)
            {
                return null;
            }

            return cowSerializer.Deserializer(cowRecords.Find(entry.Item2));
        }

        /// <summary>
        /// Find all cows that belongs to given breed and age
        /// </summary>
        public IEnumerable<CowModel> FindBy(string breed, int age)
        {
            var comparer = Comparer<Tuple<string, int>>.Default;
            var searchKey = new Tuple<string, int>(breed, age);

            // Use the secondary index to find this cow
            foreach (var entry in secondaryIndex.LargerThanOrEqualTo(searchKey))
            {
                // As soon as we reached larger key than the key given by client, stop
                if (comparer.Compare(entry.Item1, searchKey) > 0)
                {
                    break;
                }

                // Still in range, yield return
                yield return cowSerializer.Deserializer(cowRecords.Find(entry.Item2));
            }
        }

        /// <summary>
        /// Delete specified cow from our database
        /// </summary>
        public void Delete(CowModel cow)
        {
            cowRecords.Delete(primaryIndex.Get(cow.Id).Item2);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                mainDatabaseFile.Dispose();
                secondaryIndexFile.Dispose();
                primaryIndexFile.Dispose();
                disposed = true;
            }
        }

        ~CowDatabase()
        {
            Dispose(false);
        }
    }
}