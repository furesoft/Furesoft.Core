using Furesoft.Core.Storage;
using System;
using System.Collections.Generic;

namespace TestApp;

public interface ICowDatabase
{
    void Insert(CowModel cow);

    void Delete(CowModel cow);

    void Update(CowModel cow);

    CowModel Find(Guid id);

    IEnumerable<CowModel> FindBy(string breed, int age);
}