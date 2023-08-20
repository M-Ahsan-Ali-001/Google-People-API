using Google.Apis.Auth.OAuth2;
using Google.Apis.PeopleService.v1;
using Google.Apis.PeopleService.v1.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create OAuth credential.
            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = "",
                    ClientSecret = ""
                },
                new[] { "profile", "https://www.googleapis.com/auth/contacts", "https://www.googleapis.com/auth/contacts.labels" },
                "me",
                CancellationToken.None).Result;

            // Create the service.
            var service = new PeopleServiceService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "My App",
            });

            // Update contact
            //UpdateContact(service);

            // Delete contact
            //DeleteContact(service);

            // Create contact
            //CreateContact(service);

            // Create label
            // CreateLabel(service);

            // Delete label
            //DeleteLabel(service);

            // Retrieve contacts
             ListContacts(service);

            //CreateContactGroup(service, "godot");
            //DisplayContactsGroups(service);

            //UpdateTheLabel(service , "contactGroups/10acd6b70980c4cc", "MrFox");
            //DeleteContactGroup(service);
            //CreateContactGroup(service, "newHost");


            //++++new added functions-------
            ///LABEL CONTACT CRUD

            //AddContactToLabel(service, "plag", "mov", "mov2@gmail.com");
            //DeleteContactFromLabel(service, "plag" , "people/c119229067641493865");
            //UpdateContact(service);
            // ListLabelContacts(service, "plag");
            //Note  UpdateContactNote method updates and create both okay

            // UpdateContactNote(service, "", "people/c9098297175594928644", "note added agian brother!");
            // GetSpecificContactNote(service, "aa", "people/c9098297175594928644");

            DeleteContactNote(service, "", "people/c9098297175594928644");

        }

        static void UpdateContact(PeopleServiceService service)
        {
            // Retrieve the contact to be updated
            var contactToUpdate = service.People.Get("people/c119229067641493865");
            contactToUpdate.PersonFields = "names,emailAddresses"; // Specify the desired fields
            var retrievedContact = contactToUpdate.Execute();

            // Modify the contact properties
            retrievedContact.Names[0].GivenName = "i";
            retrievedContact.Names[0].FamilyName = "OSer";
            retrievedContact.Names[0].DisplayName = "no";
            retrievedContact.Names[0].DisplayNameLastFirst = "last";
            retrievedContact.EmailAddresses[0].Value = "oser1@gmail.com";

            // Perform the update operation
            var updateRequest = service.People.UpdateContact(retrievedContact, retrievedContact.ResourceName);
            updateRequest.UpdatePersonFields = "names,emailAddresses"; // Add the desired fields to update
            var updatedContact = updateRequest.Execute();
            Console.WriteLine("Contact updated: " + updatedContact.Names[0].DisplayName);
        }

        static void DeleteContact(PeopleServiceService service)
        {
            // Specify the contact ID to be deleted
            var contactId = "people/c4711090908600718814";

            // Perform the delete operation
            service.People.DeleteContact(contactId).Execute();
            Console.WriteLine("Contact deleted.");
        }

        static void CreateContact(PeopleServiceService service)
        {
            // Create a new contact object
            var newContact = new Person
            {
                Names = new List<Name>
                {
                    new Name
                    {
                        GivenName = "New Given Name",
                        FamilyName = "New Family Name"
                    }
                },
                EmailAddresses = new List<EmailAddress>
                {
                    new EmailAddress
                    {
                        Value = "new-email@example.com"
                    }
                }
            };

            // Perform the create operation
            var createdContact = service.People.CreateContact(newContact).Execute();
            Console.WriteLine("Contact created: " + createdContact.Names[0].DisplayName);
        }



        static void DeleteLabel(PeopleServiceService service)
        {
            // Specify the label ID to be deleted
            var labelId = "contactGroups/your_label_id"; // Replace with the actual label ID

            // Perform the delete operation
            service.ContactGroups.Delete(labelId).Execute();
            Console.WriteLine("Label deleted.");
        }

        static void ListContacts(PeopleServiceService service)
        {
            // Retrieve the contacts
            var peopleRequest = service.People.Connections.List("people/me");
            peopleRequest.PersonFields = "names,emailAddresses";
            var people = peopleRequest.Execute();

            // Print the contact details
            foreach (var person in people.Connections)
            {
                Console.WriteLine("Contact ID: " + person.ResourceName);
                Console.WriteLine("Display Name: " + person.Names[0].DisplayName);
                Console.WriteLine("Email: " + person.EmailAddresses[0].Value);
                Console.WriteLine();
            }
        }

       static void CreateContactGroup(PeopleServiceService Service, string label)
        {
            ContactGroup contactGroup = new ContactGroup();
            contactGroup.Name = label;
            CreateContactGroupRequest createRequest = new CreateContactGroupRequest();
            createRequest.ContactGroup = contactGroup;
            Service.ContactGroups.Create(createRequest).Execute();
        }

        static void DisplayContactsGroups(PeopleServiceService Service) {

            ListContactGroupsResponse response = Service.ContactGroups.List().Execute();
            foreach (ContactGroup group in response.ContactGroups)
            {
                Console.WriteLine("{0}: {1}", group.ResourceName, group.Name);
            }
        }

        static void DeleteContactGroup(PeopleServiceService Service) {
         Service.ContactGroups.Delete("contactGroups/6f20f97b0c40d821").Execute();
        }


        static void UpdateTheLabel(PeopleServiceService service, string contactGroupID, string newLabel)
        {
            ContactGroup contactGroup = service.ContactGroups.Get(contactGroupID).Execute();
            contactGroup.Name = newLabel;

            UpdateContactGroupRequest updateRequest = new UpdateContactGroupRequest();
            updateRequest.ContactGroup = contactGroup;

            service.ContactGroups.Update(updateRequest, contactGroupID).Execute();
        }

        static void AddContactToLabel(PeopleServiceService service, string label, string contactName, string contactEmail)
        {
            // Retrieve the contact group with the specified label
            var contactGroups = service.ContactGroups.List().Execute().ContactGroups;
            var targetGroup = contactGroups.FirstOrDefault(group => group.Name == label);

            if (targetGroup == null)
            {
                Console.WriteLine($"Contact group with label '{label}' not found.");
                return;
            }

            // Create the contact
            var contact = new Person
            {
                Names = new List<Name>
        {
            new Name { DisplayName = contactName,
             FamilyName = contactName
            }
        },
                EmailAddresses = new List<EmailAddress>
        {
            new EmailAddress { Value = contactEmail }
        }
            };

            // Add the contact to MyContacts
            var createdContact = service.People.CreateContact(contact).Execute();

            // Add the contact to the specified group
            var modifyRequest = new ModifyContactGroupMembersRequest
            {
                ResourceNamesToAdd = new List<string> { createdContact.ResourceName }
            };
            service.ContactGroups.Members.Modify(modifyRequest, targetGroup.ResourceName).Execute();
        }


        static void DeleteContactFromLabel(PeopleServiceService service, string label, string contactResourceName)
        {
            // Retrieve the contact group with the specified label
            var contactGroups = service.ContactGroups.List().Execute().ContactGroups;
            var targetGroup = contactGroups.FirstOrDefault(group => group.Name == label);

            if (targetGroup == null)
            {
                Console.WriteLine($"Contact group with label '{label}' not found.");
                return;
            }

            // Remove the contact from the specified group
            var modifyRequest = new ModifyContactGroupMembersRequest
            {
                ResourceNamesToRemove = new List<string> { contactResourceName }
            };
            service.ContactGroups.Members.Modify(modifyRequest, targetGroup.ResourceName).Execute();
        }

        static void ListLabelContacts(PeopleServiceService service, string label)
        {
            // Retrieve the contact group with the specified label
            var contactGroups = service.ContactGroups.List().Execute().ContactGroups;
            var targetGroup = contactGroups.FirstOrDefault(group => group.Name == label);

            if (targetGroup == null)
            {
                Console.WriteLine($"Contact group with label '{label}' not found.");
                return;
            }

            // Retrieve the contacts in the specified group
            var listRequest = service.People.Connections.List("people/me");
            listRequest.PersonFields = "names,emailAddresses,memberships";

            var contactsResponse = listRequest.Execute();
            var contacts = contactsResponse.Connections.Where(person => person.Memberships.Any(membership => membership.ContactGroupMembership.ContactGroupResourceName == targetGroup.ResourceName));

            Console.WriteLine($"Contacts in the '{label}' label:");

            foreach (var person in contacts)
            {
                Console.WriteLine("Contact ID: " + person.ResourceName);
                Console.WriteLine("Display Name: " + person.Names[0].DisplayName);
                Console.WriteLine("Email: " + person.EmailAddresses[0].Value);
                Console.WriteLine();
            }
        }

        static void UpdateContactNote(PeopleServiceService service, string userId, string contactResourceName, string note)
        {
            var getRequest = service.People.Get(contactResourceName);
            getRequest.PersonFields = "biographies";
            Person person = getRequest.Execute();

            person.Biographies = person.Biographies ?? new List<Biography>();
            var existingBiography = person.Biographies.FirstOrDefault(b => b.Metadata.Primary.HasValue && b.Metadata.Primary.Value);
            if (existingBiography != null)
            {
                existingBiography.Value = note;
            }
            else
            {
                person.Biographies.Add(new Biography
                {
                    Value = note,
                    Metadata = new FieldMetadata
                    {
                        Primary = true
                    }
                });
            }

            var updateRequest = service.People.UpdateContact(person, contactResourceName);
            updateRequest.UpdatePersonFields = "biographies";
            updateRequest.Execute();

            Console.WriteLine("Contact note updated.");
        }

        static string GetSpecificContactNote(PeopleServiceService service, string userId, string contactResourceName)
        {
            var getRequest = service.People.Get(contactResourceName);
            getRequest.PersonFields = "biographies";
            Person person = getRequest.Execute();

            if (person.Biographies != null && person.Biographies.Count > 0)
            {
                Console.WriteLine(person.Biographies[0].Value);
                return person.Biographies[0].Value;
            }
            return string.Empty;
        }


        static void DeleteContactNote(PeopleServiceService service, string userId, string contactResourceName)
        {
            var getRequest = service.People.Get(contactResourceName);
            getRequest.PersonFields = "biographies";
            Person person = getRequest.Execute();


            if (person.Biographies != null && person.Biographies.Count > 0)
            {
                person.Biographies.Clear();
                var updateRequest = service.People.UpdateContact(person, contactResourceName);
                updateRequest.UpdatePersonFields = "biographies";
                updateRequest.Execute();
                Console.WriteLine("Contact note deleted.");
            }
            else
            {
                Console.WriteLine("No contact note found.");
            }
        }









    }
}
