using System.Linq;
using System.Transactions;
using NUnit.Framework;
using SharpRepository.InMemoryRepository;
using SharpRepository.Tests.TestObjects;
using Should;

namespace SharpRepository.Tests.Spikes
{
	[TestFixture]
	public class MultiRepoBatchSpike
	{
		[Test]
		public void Should_Add_To_Multiple_Repos_In_Batch_After_Complete()
		{
			var contactRepository = new InMemoryRepository<Contact>();
			var emailRepository = new InMemoryRepository<EmailAddress>();
			
			contactRepository.GetAll().Count().ShouldEqual(0);
			emailRepository.GetAll().Count().ShouldEqual(0);
			
			using (var ts = new TransactionScope())
			{
				contactRepository.Add(new Contact {ContactId = 1});
				emailRepository.Add(new EmailAddress {EmailAddressId = 1});

				contactRepository.GetAll().Count().ShouldEqual(0);
				emailRepository.GetAll().Count().ShouldEqual(0);
				
				ts.Complete();
			}
			contactRepository.GetAll().Count().ShouldEqual(1);
			emailRepository.GetAll().Count().ShouldEqual(1);
		}

		[Test]
		public void Should_Perform_All_Actions_In_Batch_After_Complete()
		{
			var contactRepository = new InMemoryRepository<Contact>();
			var emailRepository = new InMemoryRepository<EmailAddress>();
			
			contactRepository.Add(new Contact {ContactId = 1, Name = "A"});
			emailRepository.Add(new EmailAddress {EmailAddressId = 1, Email = "A"});
			emailRepository.Add(new EmailAddress {EmailAddressId = 2, Email = "A"});

			contactRepository.Find(x => x.ContactId == 1).Name.ShouldEqual("A");
			emailRepository.Find(x => x.EmailAddressId == 1).Email.ShouldEqual("A");
			emailRepository.Find(x => x.EmailAddressId == 2).Email.ShouldEqual("A");

			using (var ts = new TransactionScope())
			{
				contactRepository.Update(new Contact { ContactId = 1, Name = "B" });
				emailRepository.Update(new EmailAddress {EmailAddressId = 1, Email = "B"});
				emailRepository.Delete(new EmailAddress {EmailAddressId = 2});

				contactRepository.Find(x => x.ContactId == 1).Name.ShouldEqual("A");
				emailRepository.Find(x => x.EmailAddressId == 1).Email.ShouldEqual("A");
				emailRepository.Find(x => x.EmailAddressId == 2).Email.ShouldEqual("A");

				ts.Complete();
			}
			
			contactRepository.Find(x => x.ContactId == 1).Name.ShouldEqual("B");
			emailRepository.Find(x => x.EmailAddressId == 1).Email.ShouldEqual("B");
			emailRepository.Find(x => x.EmailAddressId == 2).ShouldBeNull();
		}

		[Test]
		public void Should_Perform_No_Action_If_Complete_Not_Called()
		{
			var contactRepository = new InMemoryRepository<Contact>();
			var emailRepository = new InMemoryRepository<EmailAddress>();

			contactRepository.GetAll().Count().ShouldEqual(0);
			emailRepository.GetAll().Count().ShouldEqual(0);

			using (var ts = new TransactionScope())
			{
				contactRepository.Add(new Contact { ContactId = 1 });
				emailRepository.Add(new EmailAddress { EmailAddressId = 1 });
			}

			contactRepository.GetAll().Count().ShouldEqual(0);
			emailRepository.GetAll().Count().ShouldEqual(0);
		}

		[Test]
		public void Should_Provide_Backwards_Compatibility_For_Single_Repo_Batching()
		{
			var repo = new InMemoryRepository<Contact>();

			using (var batch = repo.BeginBatch())
			{
				batch.Add(new Contact { ContactId = 1 });
				repo.GetAll().Count().ShouldEqual(0);

				batch.Commit();
			}

			repo.GetAll().Count().ShouldEqual(1);
		}
	}
}
