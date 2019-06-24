using Newtonsoft.Json;
using StudentExercisesAPI.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace TestStudentExercisesAPI
{

    public class TestStudents
    {
        [Fact]
        public async Task Test_Get_All_Students()
        {

            using (var client = new APIClientProvider().Client)
            {
                var response = await client.GetAsync("/student");

                string responseBody = await response.Content.ReadAsStringAsync();
                var studentList = JsonConvert.DeserializeObject<List<Student>>(responseBody);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(studentList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_Student()
        {

            using (var client = new APIClientProvider().Client)
            {
                var response = await client.GetAsync("/student/1");


                string responseBody = await response.Content.ReadAsStringAsync();
                var student = JsonConvert.DeserializeObject<Student>(responseBody);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Kate", student.FirstName);
                Assert.Equal("Williams-Spradlin", student.LastName);
                Assert.NotNull(student);
            }
        }

        [Fact]
        public async Task Test_Get_NonExitant_Student_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                var response = await client.GetAsync("/student/999999999");
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }


        [Fact]
        public async Task Test_Create_And_Delete_Student()
        {
            using (var client = new APIClientProvider().Client)
            {
                Student helen = new Student
                {
                    FirstName = "Helen",
                    LastName = "Chalmers",
                    CohortId = 1,
                    SlackHandle = "Helen Chalmers"
                };
                var helenAsJSON = JsonConvert.SerializeObject(helen);


                var response = await client.PostAsync(
                    "/student",
                    new StringContent(helenAsJSON, Encoding.UTF8, "application/json")
                );


                string responseBody = await response.Content.ReadAsStringAsync();
                var newHelen = JsonConvert.DeserializeObject<Student>(responseBody);

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal("Helen", newHelen.FirstName);
                Assert.Equal("Chalmers", newHelen.LastName);
                Assert.Equal("Helen Chalmers", newHelen.SlackHandle);


                var deleteResponse = await client.DeleteAsync($"/student/{newHelen.Id}");
                Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistent_Student_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                var deleteResponse = await client.DeleteAsync("/api/students/600000");

                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_Student()
        {
            // New last name to change to and test
            string newLastName = "Williams-Spradlin";

            using (var client = new APIClientProvider().Client)
            {
                /*
                    PUT section
                 */
                Student modifiedKate = new Student
                {
                    FirstName = "Kate",
                    LastName = newLastName,
                    CohortId = 1,
                    SlackHandle = "@katerebekah"
                };
                var modifiedKateAsJSON = JsonConvert.SerializeObject(modifiedKate);

                var response = await client.PutAsync(
                    "/student/1",
                    new StringContent(modifiedKateAsJSON, Encoding.UTF8, "application/json")
                );
                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                var getKate = await client.GetAsync("/student/1");
                getKate.EnsureSuccessStatusCode();

                string getKateBody = await getKate.Content.ReadAsStringAsync();
                Student newKate = JsonConvert.DeserializeObject<Student>(getKateBody);

                Assert.Equal(HttpStatusCode.OK, getKate.StatusCode);
                Assert.Equal(newLastName, newKate.LastName);
            }
        }
    }
}
