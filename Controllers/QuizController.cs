using Microsoft.AspNetCore.Mvc;
using Elearning.Data;
using Elearning.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using System;
using DinkToPdf;
using DinkToPdf.Contracts;

namespace Elearning.Controllers
{
    public class QuizController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly IConverter _converter;

        public QuizController(ApplicationDbContext db, IWebHostEnvironment env, IConverter converter)
        {
            _db = db;
            _env = env;
            _converter = converter;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var quiz = await _db.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Questions)
                    .ThenInclude(qt => qt.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null) return NotFound();

            // Ensure questions are ordered by their Order property
            quiz.Questions = quiz.Questions.OrderBy(q => q.Order).ToList();

            return View(quiz);
        }

        [Authorize(Roles = "Admin,Formateur")]
        public async Task<IActionResult> Create(int courseId)
        {
            var course = await _db.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            var model = new Quiz { CourseId = courseId, Title = "New Quiz" };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Formateur")]
        public async Task<IActionResult> Create(Quiz model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.Quizzes.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction("Details", "Course", new { id = model.CourseId });
        }

        // Instructor: manage quiz, add questions
        [Authorize(Roles = "Admin,Formateur")]
        public async Task<IActionResult> Manage(int id)
        {
            var quiz = await _db.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Questions)
                    .ThenInclude(qt => qt.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);
            if (quiz == null) return NotFound();

            // Ensure questions are ordered
            quiz.Questions = quiz.Questions.OrderBy(q => q.Order).ToList();

            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Formateur")]
        public async Task<IActionResult> AddQuestion(int quizId, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                TempData["Error"] = "Question text cannot be empty.";
                return RedirectToAction("Manage", new { id = quizId });
            }

            var quiz = await _db.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quizId);
            if (quiz == null) return NotFound();

            // set order to append at the end
            int nextOrder = quiz.Questions.Any() ? quiz.Questions.Max(q => q.Order) + 1 : 0;

            var question = new Question { QuizId = quizId, Text = text, Order = nextOrder };
            _db.Questions.Add(question);
            await _db.SaveChangesAsync();

            return RedirectToAction("Manage", new { id = quizId });
        }

        [Authorize(Roles = "Admin,Formateur")]
        public async Task<IActionResult> EditQuestion(int id)
        {
            var question = await _db.Questions
                .Include(q => q.Answers)
                .Include(q => q.Quiz)
                .FirstOrDefaultAsync(q => q.Id == id);
            if (question == null) return NotFound();
            return View(question);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Formateur")]
        public async Task<IActionResult> EditQuestion(int id, string text)
        {
            var question = await _db.Questions.FindAsync(id);
            if (question == null) return NotFound();
            if (string.IsNullOrWhiteSpace(text))
            {
                ModelState.AddModelError("Text", "Question text cannot be empty.");
                // reload answers for the view
                question = await _db.Questions.Include(q => q.Answers).FirstOrDefaultAsync(q => q.Id == id);
                return View(question);
            }

            question.Text = text;
            _db.Questions.Update(question);
            await _db.SaveChangesAsync();
            return RedirectToAction("Manage", new { id = question.QuizId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Formateur")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _db.Questions.FindAsync(id);
            if (question == null) return NotFound();
            var quizId = question.QuizId;
            _db.Questions.Remove(question);
            await _db.SaveChangesAsync();
            return RedirectToAction("Manage", new { id = quizId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Formateur")]
        public async Task<IActionResult> AddAnswer(int questionId, string text, bool isCorrect = false)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                TempData["Error"] = "Answer text cannot be empty.";
                var q = await _db.Questions.FindAsync(questionId);
                return RedirectToAction("EditQuestion", new { id = questionId });
            }

            var question = await _db.Questions.Include(q => q.Quiz).Include(q => q.Answers).FirstOrDefaultAsync(q => q.Id == questionId);
            if (question == null) return NotFound();

            // If adding a non-correct answer while there are currently no correct answers, disallow
            if (!isCorrect && !question.Answers.Any(a => a.IsCorrect))
            {
                TempData["Error"] = "Each question must have at least one correct answer. Mark an answer as correct.";
                return RedirectToAction("EditQuestion", new { id = questionId });
            }

            var answer = new Answer { QuestionId = questionId, Text = text, IsCorrect = isCorrect };
            _db.Answers.Add(answer);
            await _db.SaveChangesAsync();

            return RedirectToAction("EditQuestion", new { id = questionId });
        }

        [Authorize(Roles = "Admin,Formateur")]
        public async Task<IActionResult> EditAnswer(int id)
        {
            var answer = await _db.Answers.Include(a => a.Question).FirstOrDefaultAsync(a => a.Id == id);
            if (answer == null) return NotFound();
            return View(answer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Formateur")]
        public async Task<IActionResult> EditAnswer(int id, string text, bool isCorrect = false)
        {
            var answer = await _db.Answers.FindAsync(id);
            if (answer == null) return NotFound();
            if (string.IsNullOrWhiteSpace(text))
            {
                ModelState.AddModelError("Text", "Answer text cannot be empty.");
                answer = await _db.Answers.Include(a => a.Question).FirstOrDefaultAsync(a => a.Id == id);
                return View(answer);
            }

            // Check if unchecking this answer would leave the question withouth any correct answers
            if (!isCorrect)
            {
                var otherCorrectCount = await _db.Answers.CountAsync(a => a.QuestionId == answer.QuestionId && a.IsCorrect && a.Id != answer.Id);
                if (otherCorrectCount == 0)
                {
                    TempData["Error"] = "Each question must have at least one correct answer. You cannot unmark the last correct answer.";
                    return RedirectToAction("EditQuestion", new { id = answer.QuestionId });
                }
            }

            answer.Text = text;
            answer.IsCorrect = isCorrect;
            _db.Answers.Update(answer);
            await _db.SaveChangesAsync();
            return RedirectToAction("EditQuestion", new { id = answer.QuestionId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Formateur")]
        public async Task<IActionResult> DeleteAnswer(int id)
        {
            var answer = await _db.Answers.FindAsync(id);
            if (answer == null) return NotFound();

            // Check if deleting this answer would remove the only correct answer
            if (answer.IsCorrect)
            {
                var correctCount = await _db.Answers.CountAsync(a => a.QuestionId == answer.QuestionId && a.IsCorrect && a.Id != answer.Id);
                if (correctCount == 0)
                {
                    TempData["Error"] = "Each question must have at least one correct answer. You cannot delete the last correct answer.";
                    return RedirectToAction("EditQuestion", new { id = answer.QuestionId });
                }
            }

            var questionId = answer.QuestionId;
            _db.Answers.Remove(answer);
            await _db.SaveChangesAsync();
            return RedirectToAction("EditQuestion", new { id = questionId });
        }

        // AJAX endpoint to reorder questions
        [HttpPost]
        [Authorize(Roles = "Admin,Formateur")]
        public async Task<IActionResult> ReorderQuestions(int quizId, [FromBody] List<int> orderedIds)
        {
            if (orderedIds == null) return BadRequest();
            var questions = await _db.Questions.Where(q => q.QuizId == quizId && orderedIds.Contains(q.Id)).ToListAsync();
            for (int i = 0; i < orderedIds.Count; i++)
            {
                var q = questions.FirstOrDefault(x => x.Id == orderedIds[i]);
                if (q != null)
                {
                    q.Order = i;
                }
            }

            _db.Questions.UpdateRange(questions);
            await _db.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [Authorize]
        public async Task<IActionResult> Take(int id)
        {
            var quiz = await _db.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qt => qt.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);
            if (quiz == null) return NotFound();

            quiz.Questions = quiz.Questions.OrderBy(q => q.Order).ToList();

            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Submit(int quizId)
        {
            var quiz = await _db.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qt => qt.Answers)
                .FirstOrDefaultAsync(q => q.Id == quizId);
            if (quiz == null) return NotFound();

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId)) return Forbid();

            int totalQuestions = quiz.Questions.Count;
            int correctCount = 0;

            foreach (var question in quiz.Questions)
            {
                var formKey = $"answer_{question.Id}";
                if (!Request.Form.ContainsKey(formKey)) continue;
                var value = Request.Form[formKey].ToString();
                if (int.TryParse(value, out var selectedAnswerId))
                {
                    var selected = question.Answers.FirstOrDefault(a => a.Id == selectedAnswerId);
                    if (selected != null && selected.IsCorrect) correctCount++;
                }
            }

            int score = correctCount;
            int maxScore = totalQuestions;
            double percentage = maxScore == 0 ? 0 : (double)score / maxScore * 100.0;
            bool passed = percentage >= quiz.PassPercentage;

            var result = new Result
            {
                UserId = userId,
                QuizId = quiz.Id,
                Score = score,
                MaxScore = maxScore,
                Passed = passed,
                TakenAt = DateTime.UtcNow
            };

            _db.Results.Add(result);
            await _db.SaveChangesAsync();

            if (passed)
            {
                // create certificate record
                var cert = new Certificate
                {
                    UserId = userId,
                    CourseId = quiz.CourseId,
                    ResultId = result.Id,
                    Score = score,
                    MaxScore = maxScore,
                    Passed = true,
                    IssuedAt = DateTime.UtcNow
                };
                _db.Certificates.Add(cert);
                await _db.SaveChangesAsync();

                var certsFolder = Path.Combine(_env.WebRootPath, "certificates");
                if (!Directory.Exists(certsFolder)) Directory.CreateDirectory(certsFolder);
                var fileName = $"cert_{cert.Id}_{cert.VerificationCode}.pdf";
                var filePath = Path.Combine(certsFolder, fileName);

                var user = await _db.Users.FindAsync(userId);

                // Generate an improved HTML certificate (DinkToPdf will convert on download)
                var htmlFileName = $"cert_{cert.Id}_{cert.VerificationCode}.html";
                var htmlFilePath = Path.Combine(certsFolder, htmlFileName);

                var userName = System.Net.WebUtility.HtmlEncode(user?.FullName ?? "Learner");
                var courseTitle = System.Net.WebUtility.HtmlEncode(quiz.Title);
                var issuedDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
                var verificationUrl = Url.Action("VerifyCertificate", "Quiz", new { code = cert.VerificationCode }, Request.Scheme);
                var verificationCode = System.Net.WebUtility.HtmlEncode(cert.VerificationCode);

                var html = $@"<!doctype html>
<html>
<head>
  <meta charset='utf-8' />
  <meta name='viewport' content='width=device-width,initial-scale=1' />
  <title>Certificate of Completion</title>
  <style>
    @media print {{ body {{ -webkit-print-color-adjust: exact; }} }}
    body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial; color: #0b2b3a; margin:0; padding:0; background: #f6f8fa; }}
    .page {{ width:100%; max-width:1000px; margin:30px auto; padding:28px; box-sizing:border-box; }}
    .certificate {{ background: white; padding:36px; border-radius:14px; box-shadow: 0 18px 50px rgba(11,43,58,0.06); border: 1px solid rgba(11,43,58,0.04); position:relative; overflow:hidden; }}

    /* Decorative corner */
    .corner {{ position:absolute; right:-120px; top:-120px; width:320px; height:320px; background: linear-gradient(135deg, rgba(43,108,176,0.06), rgba(43,108,176,0.02)); transform:rotate(25deg); opacity:0.9; }}

    .header {{ display:flex; align-items:center; gap:18px; margin-bottom:4px; }}
    .brand {{ display:flex; flex-direction:column; justify-content:center; }}
    .brand .org {{ font-weight:700; color:#1f4e79; font-size:18px; letter-spacing:0.4px; }}
    .brand .tag {{ color:#6b7b83; font-size:12px; margin-top:2px; }}

    .ribbon {{ display:inline-block; padding:6px 12px; background: linear-gradient(90deg,#f6c85f,#f0b429); color:#3a2b00; font-weight:700; border-radius:999px; box-shadow: 0 6px 18px rgba(240,180,41,0.12); font-size:12px; }}

    .title {{ text-align:center; margin-top:8px; }}
    .title h1 {{ font-size:32px; margin:6px 0; color:#0b2b3a; }}
    .title p.lead {{ margin:0; color:#516977; font-size:14px; }}

    .recipient {{ margin-top:18px; text-align:center; }}
    .recipient .name {{ font-size:30px; font-weight:700; color:#0b2430; margin:8px 0; letter-spacing:-0.4px; }}
    .recipient .meta {{ color:#516977; font-size:14px; margin-bottom:6px; }}
    .recipient .course {{ font-size:18px; font-weight:600; color:#133349; margin-top:8px; }}

    .details {{ display:flex; gap:18px; justify-content:center; margin-top:28px; flex-wrap:wrap; }}
    .stat {{ min-width:160px; padding:14px 18px; border-radius:10px; background:linear-gradient(180deg,#fbfdff,#f8fbff); border:1px solid rgba(11,43,58,0.04); text-align:center; box-shadow: 0 6px 18px rgba(11,43,58,0.04); }}
    .stat .big {{ font-size:20px; font-weight:700; color:#0b2b3a; }}
    .stat .small {{ font-size:12px; color:#657f8d; margin-top:6px; }}

    .footer {{ margin-top:28px; display:flex; justify-content:space-between; align-items:center; gap:12px; flex-wrap:wrap; }}
    .signature {{ text-align:left; }}
    .sig-line {{ width:220px; height:1px; background:linear-gradient(90deg, rgba(11,43,58,0.12), rgba(11,43,58,0)); margin-top:18px; }}
    .verify {{ text-align:right; font-size:12px; color:#2b6cb0; }}
    .verify a {{ color:#2b6cb0; text-decoration:none; }}

    .small-note {{ margin-top:10px; font-size:11px; color:#9aa8b4; }}

    @media (max-width:520px) {{ .details {{ flex-direction:column; align-items:center; }} .verify {{ text-align:left; }} }}
  </style>
</head>
<body>
  <div class='page'>
    <div class='certificate'>
      <div class='corner' aria-hidden='true'></div>

      <div class='header'>
        <div class='brand'>
          <div class='org'>Informatique</div>
          <div class='tag'>Practical online courses & certifications</div>
        </div>
        <div style='flex:1'></div>
        <div class='ribbon'>Certificate</div>
      </div>

      <div class='title'>
        <h1>Certificate of Completion</h1>
        <p class='lead'>This certifies successful completion of the course</p>
      </div>

      <div class='recipient'>
        <div class='meta'>Presented to</div>
        <div class='name'>{userName}</div>
        <div class='course'>{courseTitle}</div>
      </div>

      <div class='details'>
        <div class='stat'>
          <div class='big'>{score}/{maxScore}</div>
          <div class='small'>Score</div>
        </div>
        <div class='stat'>
          <div class='big'>{(passed ? "Passed" : "Completed")}</div>
          <div class='small'>Result</div>
        </div>
        <div class='stat'>
          <div class='big'>{issuedDate}</div>
          <div class='small'>Issued</div>
        </div>
      </div>

      <div class='footer'>
        <div class='signature'>
          <div style='font-size:13px; color:#516977;'>Authorized by</div>
          <div style='font-weight:700; margin-top:6px;'>Informatique Team</div>
          <div class='sig-line' aria-hidden='true'></div>
        </div>

        <div class='verify'>
          <div style='font-size:12px; color:#6b7b83;'>Verification code</div>
          <div style='font-weight:700; margin-top:6px; color:#153d57;'>{verificationCode}</div>
          <div class='small-note'>Verify at: <a href='{System.Net.WebUtility.HtmlEncode(verificationUrl)}'>{System.Net.WebUtility.HtmlEncode(verificationUrl)}</a></div>
        </div>
      </div>

    </div>
  </div>
</body>
</html>";

                await System.IO.File.WriteAllTextAsync(htmlFilePath, html);
                cert.FilePath = Path.Combine("certificates", htmlFileName).Replace("\\", "/");

                _db.Certificates.Update(cert);
                await _db.SaveChangesAsync();

                return RedirectToAction("DownloadCertificate", new { id = cert.Id });
            }

            return RedirectToAction("Result", new { id = result.Id });
        }

        [Authorize]
        public async Task<IActionResult> DownloadCertificate(int id)
        {
            var cert = await _db.Certificates.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
            if (cert == null) return NotFound();

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId)) return Forbid();
            if (cert.UserId != userId && !User.IsInRole("Admin")) return Forbid();

            var path = Path.Combine(_env.WebRootPath, cert.FilePath ?? string.Empty);
            if (!System.IO.File.Exists(path)) return NotFound();

            var ext = Path.GetExtension(path)?.ToLowerInvariant();

            // If stored file is HTML, try to convert to PDF on the fly using DinkToPdf
            if (ext == ".html")
            {
                try
                {
                    var htmlContent = await System.IO.File.ReadAllTextAsync(path);

                    var doc = new HtmlToPdfDocument()
                    {
                        GlobalSettings = {
                            ColorMode = ColorMode.Color,
                            Orientation = Orientation.Portrait,
                            PaperSize = PaperKind.A4,
                            Margins = new MarginSettings { Top = 20, Bottom = 20 }
                        },
                        Objects = {
                            new ObjectSettings
                            {
                                HtmlContent = htmlContent,
                                WebSettings = { DefaultEncoding = "utf-8" }
                            }
                        }
                    };

                    byte[] pdf = _converter.Convert(doc);
                    if (pdf != null && pdf.Length > 0)
                    {
                        // save generated pdf next to html file
                        var pdfFileName = Path.ChangeExtension(path, ".pdf");
                        await System.IO.File.WriteAllBytesAsync(pdfFileName, pdf);

                        // update certificate record to point to pdf file
                        cert.FilePath = Path.GetRelativePath(_env.WebRootPath, pdfFileName).Replace("\\", "/");
                        _db.Certificates.Update(cert);
                        await _db.SaveChangesAsync();

                        return File(pdf, "application/pdf", Path.GetFileName(pdfFileName));
                    }
                }
                catch (Exception)
                {
                    // conversion failed, fall back to returning the HTML
                    var bytesHtml = await System.IO.File.ReadAllBytesAsync(path);
                    return File(bytesHtml, "text/html", Path.GetFileName(path));
                }
            }

            // For PDF files or if conversion not needed
            var bytes = await System.IO.File.ReadAllBytesAsync(path);
            var contentType = ext == ".html" ? "text/html" : "application/pdf";
            return File(bytes, contentType, Path.GetFileName(path));
        }

        [AllowAnonymous]
        public async Task<IActionResult> VerifyCertificate(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return BadRequest();
            var cert = await _db.Certificates.Include(c => c.User).Include(c => c.Course).FirstOrDefaultAsync(c => c.VerificationCode == code);
            if (cert == null) return NotFound(new { valid = false });
            return Ok(new { valid = true, issuedAt = cert.IssuedAt, user = cert.User?.FullName, course = cert.Course?.Title, score = cert.Score, maxScore = cert.MaxScore, passed = cert.Passed });
        }

        [Authorize]
        public async Task<IActionResult> Result(int id)
        {
            var result = await _db.Results
                .Include(r => r.Quiz)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (result == null) return NotFound();

            // Ensure the user can only view their own result unless Admin
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId)) return Forbid();
            if (result.UserId != userId && !User.IsInRole("Admin")) return Forbid();

            return View(result);
        }
    }
}
