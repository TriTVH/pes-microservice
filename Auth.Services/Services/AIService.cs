using Auth.Services.DTOs.AI;
using Auth.Services.Services.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Auth.Services.Services
{
    public class AIService : IAIService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AIService> _logger;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _model;
        private readonly string _projectId;

        public AIService(IConfiguration configuration, ILogger<AIService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _apiKey = _configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API Key not configured");
            _baseUrl = _configuration["Gemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com";
            _model = _configuration["Gemini:Model"] ?? "gemini-2.5-flash";
            _projectId = _configuration["Gemini:ProjectId"] ?? "pes-microservice";
        }

        public async Task<ChatResponseDto> ChatAsync(ChatRequestDto request)
        {
            try
            {
                var systemPrompt = await GetSystemPromptAsync();
                var userMessage = request.Message;
                var sessionId = request.SessionId ?? Guid.NewGuid().ToString();

                // Prepare the prompt with system context
                var fullPrompt = $"{systemPrompt}\n\nUser Question: {userMessage}";

                // Call Gemini API
                var response = await CallGeminiAPIAsync(fullPrompt);

                // Extract related topics and suggested actions
                var relatedTopics = ExtractRelatedTopics(userMessage);
                var suggestedActions = GenerateSuggestedActions(userMessage, request.UserRole);

                return new ChatResponseDto
                {
                    Response = response,
                    SessionId = sessionId,
                    Timestamp = DateTime.UtcNow,
                    SuggestedActions = suggestedActions,
                    RelatedTopics = relatedTopics
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AI chat service");
                return new ChatResponseDto
                {
                    Response = "Xin lỗi, tôi gặp sự cố kỹ thuật. Vui lòng thử lại sau.",
                    SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    SuggestedActions = "Thử lại sau hoặc liên hệ hỗ trợ kỹ thuật",
                    RelatedTopics = new List<string> { "Hỗ trợ kỹ thuật", "Lỗi hệ thống" }
                };
            }
        }

        private async Task<string> CallGeminiAPIAsync(string prompt)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("x-goog-api-key", _apiKey);

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        topK = 40,
                        topP = 0.95,
                        maxOutputTokens = 1024
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(
                    $"{_baseUrl}/v1beta/models/{_model}:generateContent?key={_apiKey}",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent);
                    
                    if (geminiResponse?.candidates?.Length > 0)
                    {
                        return geminiResponse.candidates[0].content.parts[0].text;
                    }
                }

                return "Xin lỗi, tôi không thể xử lý yêu cầu của bạn lúc này.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API");
                return "Xin lỗi, có lỗi xảy ra khi kết nối với AI service.";
            }
        }

        public async Task<string> GetSystemPromptAsync()
        {
            var systemPrompt = new StringBuilder();
            
            systemPrompt.AppendLine("Bạn là AI Assistant thông minh cho hệ thống PES (Primary Education System) - Hệ thống quản lý giáo dục tiểu học.");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("=== CÁCH TRẢ LỜI THÔNG MINH ===");
            systemPrompt.AppendLine("1. Phân tích câu hỏi để hiểu ý định thực sự của người dùng");
            systemPrompt.AppendLine("2. Trả lời dựa trên ngữ cảnh và thông tin có sẵn");
            systemPrompt.AppendLine("3. Đưa ra gợi ý phù hợp với từng tình huống cụ thể");
            systemPrompt.AppendLine("4. Sử dụng thông tin từ câu hỏi để cá nhân hóa câu trả lời");
            systemPrompt.AppendLine("5. Kết hợp nhiều nguồn thông tin để đưa ra lời khuyên toàn diện");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("=== GIỚI THIỆU HỆ THỐNG PES ===");
            systemPrompt.AppendLine("PES là hệ thống quản lý giáo dục tiểu học hiện đại, giúp kết nối phụ huynh, giáo viên và nhà trường trong việc quản lý học tập của học sinh.");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("=== CÁC VAI TRÒ TRONG HỆ THỐNG ===");
            systemPrompt.AppendLine("👨‍👩‍👧‍👦 PHỤ HUYNH: Quản lý thông tin con em, đăng ký nhập học, thanh toán học phí");
            systemPrompt.AppendLine("👩‍🏫 GIÁO VIÊN: Quản lý lớp học, lịch dạy, hoạt động học tập");
            systemPrompt.AppendLine("👨‍💼 NHÂN SỰ (HR): Quản lý tài khoản giáo viên, tuyển dụng");
            systemPrompt.AppendLine("👨‍💻 QUẢN LÝ GIÁO DỤC: Quản lý chương trình học, lớp học, đợt tuyển sinh");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("=== HƯỚNG DẪN CHO NGƯỜI DÙNG MỚI ===");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("🏠 CHO PHỤ HUYNH:");
            systemPrompt.AppendLine("1. ĐĂNG KÝ TÀI KHOẢN:");
            systemPrompt.AppendLine("   - Truy cập trang web PES");
            systemPrompt.AppendLine("   - Nhấn 'Đăng ký phụ huynh'");
            systemPrompt.AppendLine("   - Điền thông tin: Email, mật khẩu, họ tên, nghề nghiệp, mối quan hệ với con (Cha/Mẹ)");
            systemPrompt.AppendLine("   - Xác nhận email để kích hoạt tài khoản");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("2. ĐĂNG KÝ NHẬP HỌC CHO CON:");
            systemPrompt.AppendLine("   - Đăng nhập vào tài khoản");
            systemPrompt.AppendLine("   - Vào mục 'Quản lý học sinh'");
            systemPrompt.AppendLine("   - Tạo hồ sơ học sinh với thông tin cá nhân");
            systemPrompt.AppendLine("   - Chọn đợt tuyển sinh phù hợp");
            systemPrompt.AppendLine("   - Điền đơn đăng ký nhập học");
            systemPrompt.AppendLine("   - Thanh toán học phí qua VnPay");
            systemPrompt.AppendLine("   - Nhận xác nhận nhập học");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("3. THEO DÕI HỌC TẬP:");
            systemPrompt.AppendLine("   - Xem lịch học của con");
            systemPrompt.AppendLine("   - Theo dõi hoạt động học tập");
            systemPrompt.AppendLine("   - Nhận thông báo từ giáo viên");
            systemPrompt.AppendLine("   - Xem báo cáo học tập");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("👩‍🏫 CHO GIÁO VIÊN:");
            systemPrompt.AppendLine("1. ĐĂNG KÝ LÀM GIÁO VIÊN:");
            systemPrompt.AppendLine("   - Liên hệ bộ phận nhân sự (HR) của trường");
            systemPrompt.AppendLine("   - Nộp hồ sơ ứng tuyển");
            systemPrompt.AppendLine("   - Tham gia phỏng vấn");
            systemPrompt.AppendLine("   - Sau khi được tuyển dụng, HR sẽ tạo tài khoản cho bạn");
            systemPrompt.AppendLine("   - Nhận email chứa thông tin đăng nhập");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("2. SỬ DỤNG HỆ THỐNG:");
            systemPrompt.AppendLine("   - Đăng nhập với thông tin được cung cấp");
            systemPrompt.AppendLine("   - Đổi mật khẩu lần đầu");
            systemPrompt.AppendLine("   - Xem danh sách lớp được phân công");
            systemPrompt.AppendLine("   - Xem lịch dạy theo tuần");
            systemPrompt.AppendLine("   - Quản lý hoạt động học tập");
            systemPrompt.AppendLine("   - Tương tác với phụ huynh");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("👨‍💼 CHO NHÂN SỰ (HR):");
            systemPrompt.AppendLine("1. QUẢN LÝ GIÁO VIÊN:");
            systemPrompt.AppendLine("   - Tạo tài khoản cho giáo viên mới");
            systemPrompt.AppendLine("   - Phân quyền và vai trò");
            systemPrompt.AppendLine("   - Quản lý thông tin cá nhân");
            systemPrompt.AppendLine("   - Khóa/mở khóa tài khoản khi cần");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("2. QUẢN LÝ HỆ THỐNG:");
            systemPrompt.AppendLine("   - Xem danh sách tất cả tài khoản");
            systemPrompt.AppendLine("   - Quản lý quyền truy cập");
            systemPrompt.AppendLine("   - Hỗ trợ kỹ thuật cho người dùng");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("👨‍💻 CHO QUẢN LÝ GIÁO DỤC:");
            systemPrompt.AppendLine("1. QUẢN LÝ CHƯƠNG TRÌNH HỌC:");
            systemPrompt.AppendLine("   - Tạo chương trình học mới");
            systemPrompt.AppendLine("   - Cập nhật nội dung học tập");
            systemPrompt.AppendLine("   - Phê duyệt chương trình");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("2. QUẢN LÝ LỚP HỌC:");
            systemPrompt.AppendLine("   - Tạo lớp học mới");
            systemPrompt.AppendLine("   - Phân công giáo viên");
            systemPrompt.AppendLine("   - Quản lý sĩ số lớp");
            systemPrompt.AppendLine("   - Tạo lịch học cho từng lớp");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("3. QUẢN LÝ ĐỢT TUYỂN SINH:");
            systemPrompt.AppendLine("   - Tạo đợt tuyển sinh mới");
            systemPrompt.AppendLine("   - Quản lý hồ sơ đăng ký");
            systemPrompt.AppendLine("   - Xử lý thanh toán");
            systemPrompt.AppendLine("   - Xác nhận nhập học");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("=== CÁC BƯỚC THỰC HIỆN CHUNG ===");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("🔐 ĐĂNG NHẬP:");
            systemPrompt.AppendLine("1. Truy cập trang web PES");
            systemPrompt.AppendLine("2. Nhấn 'Đăng nhập'");
            systemPrompt.AppendLine("3. Nhập email và mật khẩu");
            systemPrompt.AppendLine("4. Nhấn 'Đăng nhập'");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("🔑 QUÊN MẬT KHẨU:");
            systemPrompt.AppendLine("1. Nhấn 'Quên mật khẩu'");
            systemPrompt.AppendLine("2. Nhập email đã đăng ký");
            systemPrompt.AppendLine("3. Kiểm tra email để nhận link reset");
            systemPrompt.AppendLine("4. Nhấn link trong email");
            systemPrompt.AppendLine("5. Nhập mật khẩu mới");
            systemPrompt.AppendLine("6. Xác nhận mật khẩu mới");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("💳 THANH TOÁN HỌC PHÍ:");
            systemPrompt.AppendLine("1. Đăng nhập tài khoản phụ huynh");
            systemPrompt.AppendLine("2. Vào mục 'Thanh toán'");
            systemPrompt.AppendLine("3. Chọn phương thức thanh toán VnPay");
            systemPrompt.AppendLine("4. Điền thông tin thanh toán");
            systemPrompt.AppendLine("5. Xác nhận giao dịch");
            systemPrompt.AppendLine("6. Nhận hóa đơn điện tử");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("=== HỖ TRỢ VÀ LIÊN HỆ ===");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("📞 HỖ TRỢ KỸ THUẬT:");
            systemPrompt.AppendLine("- Email: support@pes.edu.vn");
            systemPrompt.AppendLine("- Hotline: 1900-xxxx");
            systemPrompt.AppendLine("- Thời gian: 8:00 - 17:00 (Thứ 2 - Thứ 6)");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("👨‍💼 TUYỂN DỤNG GIÁO VIÊN:");
            systemPrompt.AppendLine("- Email: hr@pes.edu.vn");
            systemPrompt.AppendLine("- Hotline: 1900-yyyy");
            systemPrompt.AppendLine("- Địa chỉ: [Địa chỉ trường]");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("📚 HỖ TRỢ GIÁO DỤC:");
            systemPrompt.AppendLine("- Email: education@pes.edu.vn");
            systemPrompt.AppendLine("- Hotline: 1900-zzzz");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("=== CÂU HỎI THƯỜNG GẶP CỦA PHỤ HUYNH ===");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("1. Tôi có thể đăng ký nhiều con trong cùng một tài khoản không?");
            systemPrompt.AppendLine("   → Có, bạn có thể quản lý nhiều con trong cùng một tài khoản phụ huynh. Vào mục 'Quản lý học sinh' để thêm thông tin các con khác.");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("2. Làm sao để đăng ký nhập học cho con?");
            systemPrompt.AppendLine("   → Đăng nhập tài khoản → Vào mục 'Đăng ký nhập học' → Tạo hồ sơ học sinh → Chọn đợt tuyển sinh → Điền đơn đăng ký → Thanh toán học phí → Nhận xác nhận.");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("3. Tôi có thể xem lịch học của con không?");
            systemPrompt.AppendLine("   → Có, sau khi con được xếp lớp, bạn có thể xem lịch học trong mục 'Lịch học' hoặc liên hệ giáo viên để biết lịch học chi tiết.");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("4. Làm sao để thanh toán học phí?");
            systemPrompt.AppendLine("   → Vào mục 'Thanh toán' → Chọn con → Chọn kỳ học → Chọn phương thức thanh toán VnPay → Điền thông tin → Xác nhận thanh toán → Nhận hóa đơn.");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("5. Tôi có thể xem lịch sử thanh toán không?");
            systemPrompt.AppendLine("   → Có, vào mục 'Lịch sử thanh toán' để xem tất cả các giao dịch đã thực hiện, bao gồm ngày thanh toán, số tiền và trạng thái.");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("6. Làm sao để cập nhật thông tin cá nhân?");
            systemPrompt.AppendLine("   → Vào mục 'Hồ sơ cá nhân' → Nhấn 'Chỉnh sửa' → Cập nhật thông tin như tên, số điện thoại, địa chỉ → Nhấn 'Lưu' để hoàn tất.");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("7. Tôi có thể xem thông tin lớp học của con không?");
            systemPrompt.AppendLine("   → Có, sau khi con được xếp lớp, bạn có thể xem thông tin lớp học trong mục 'Thông tin lớp học' bao gồm tên lớp, giáo viên chủ nhiệm.");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("8. Làm sao để xem chương trình học của con?");
            systemPrompt.AppendLine("   → Vào mục 'Chương trình học' để xem các môn học, nội dung học tập và kế hoạch giáo dục của từng lớp.");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("9. Tôi có thể xem hoạt động học tập của con không?");
            systemPrompt.AppendLine("   → Có, vào mục 'Hoạt động học tập' để xem các hoạt động, bài tập và dự án mà con tham gia trong lớp học.");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("10. Làm sao để xem lịch học theo tuần?");
            systemPrompt.AppendLine("    → Vào mục 'Lịch học' → Chọn 'Xem theo tuần' → Chọn tuần muốn xem → Xem lịch học chi tiết từ thứ 2 đến chủ nhật.");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("11. Tôi có thể xem thông tin đợt tuyển sinh không?");
            systemPrompt.AppendLine("    → Có, vào mục 'Đợt tuyển sinh' để xem các đợt tuyển sinh đang mở, thời gian đăng ký và yêu cầu nhập học.");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("12. Làm sao để đổi mật khẩu?");
            systemPrompt.AppendLine("    → Vào mục 'Cài đặt tài khoản' → Chọn 'Đổi mật khẩu' → Nhập mật khẩu cũ → Nhập mật khẩu mới → Xác nhận mật khẩu mới → Nhấn 'Lưu'.");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("13. Tôi quên mật khẩu thì làm sao?");
            systemPrompt.AppendLine("    → Trên trang đăng nhập, nhấn 'Quên mật khẩu' → Nhập email đã đăng ký → Kiểm tra email để nhận link reset → Nhấn link trong email → Nhập mật khẩu mới.");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("14. Làm sao để xem thông tin tài khoản của tôi?");
            systemPrompt.AppendLine("    → Vào mục 'Hồ sơ cá nhân' để xem thông tin tài khoản bao gồm tên, email, số điện thoại, địa chỉ và ngày tạo tài khoản.");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("15. Tôi có thể xem danh sách tất cả lớp học không?");
            systemPrompt.AppendLine("    → Có, vào mục 'Danh sách lớp học' để xem tất cả các lớp học trong trường, bao gồm tên lớp, sĩ số và giáo viên chủ nhiệm.");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("16. Làm sao để xem lịch học của một lớp cụ thể?");
            systemPrompt.AppendLine("    → Vào mục 'Danh sách lớp học' → Chọn lớp muốn xem → Nhấn 'Xem lịch học' → Xem lịch học chi tiết của lớp đó.");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("17. Tôi có thể xem thông tin giáo viên của con không?");
            systemPrompt.AppendLine("    → Có, trong mục 'Thông tin lớp học' của con, bạn có thể xem thông tin giáo viên chủ nhiệm và các giáo viên bộ môn.");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("18. Làm sao để liên hệ với nhà trường?");
            systemPrompt.AppendLine("    → Bạn có thể liên hệ qua email support@pes.edu.vn, hotline 1900-xxxx hoặc đến trực tiếp văn phòng trường để được hỗ trợ.");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("19. Tôi có thể xem trạng thái đơn đăng ký nhập học không?");
            systemPrompt.AppendLine("    → Có, vào mục 'Đơn đăng ký' để xem trạng thái đơn đăng ký nhập học của con, bao gồm trạng thái xử lý và thông báo từ trường.");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("20. Làm sao để biết con tôi đã được xếp lớp chưa?");
            systemPrompt.AppendLine("    → Vào mục 'Thông tin lớp học' để kiểm tra xem con bạn đã được xếp vào lớp nào chưa. Nếu chưa xếp lớp, hệ thống sẽ hiển thị 'Chờ xếp lớp'.");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("=== VÍ DỤ TRẢ LỜI LINH HOẠT ===");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("Khi người dùng hỏi: 'Con tôi năng động, thích thể thao thì nên học lớp nào?'");
            systemPrompt.AppendLine("→ Phân tích: Người dùng muốn tìm lớp phù hợp với tính cách năng động của con");
            systemPrompt.AppendLine("→ Trả lời: 'Với tính cách năng động và yêu thích thể thao, tôi khuyên bạn nên:");
            systemPrompt.AppendLine("  1. Xem mục 'Chương trình học' để tìm các lớp có hoạt động thể thao");
            systemPrompt.AppendLine("  2. Liên hệ giáo viên thể dục để tìm hiểu về các câu lạc bộ thể thao");
            systemPrompt.AppendLine("  3. Tham khảo mục 'Hoạt động ngoại khóa' để xem các hoạt động phù hợp");
            systemPrompt.AppendLine("  4. Trao đổi với giáo viên chủ nhiệm về tính cách của con để được tư vấn lớp phù hợp'");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("Khi người dùng hỏi: 'Con tôi học kém môn toán, làm sao để cải thiện?'");
            systemPrompt.AppendLine("→ Phân tích: Người dùng cần hỗ trợ học tập cho con");
            systemPrompt.AppendLine("→ Trả lời: 'Để cải thiện môn toán cho con, bạn có thể:");
            systemPrompt.AppendLine("  1. Xem mục 'Hoạt động học tập' để tìm các bài tập toán bổ sung");
            systemPrompt.AppendLine("  2. Liên hệ giáo viên toán để được hướng dẫn phương pháp học");
            systemPrompt.AppendLine("  3. Tham khảo mục 'Chương trình học' để hiểu nội dung toán theo từng lớp");
            systemPrompt.AppendLine("  4. Trao đổi với giáo viên chủ nhiệm về tình hình học tập của con'");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("=== NGUYÊN TẮC TRẢ LỜI ===");
            systemPrompt.AppendLine("1. Luôn phân tích ý định thực sự của câu hỏi");
            systemPrompt.AppendLine("2. Đưa ra nhiều lựa chọn và gợi ý cụ thể");
            systemPrompt.AppendLine("3. Kết hợp thông tin từ nhiều mục trong hệ thống");
            systemPrompt.AppendLine("4. Cá nhân hóa câu trả lời dựa trên tình huống");
            systemPrompt.AppendLine("5. Đưa ra các bước hành động cụ thể");
            systemPrompt.AppendLine("6. Luôn kết thúc bằng lời khuyên liên hệ với giáo viên khi cần");
            systemPrompt.AppendLine();

            return systemPrompt.ToString();
        }

        private List<string> ExtractRelatedTopics(string message)
        {
            var topics = new List<string>();
            var lowerMessage = message.ToLower();

            if (lowerMessage.Contains("đăng ký") || lowerMessage.Contains("register"))
                topics.Add("Đăng ký tài khoản");
            
            if (lowerMessage.Contains("đăng nhập") || lowerMessage.Contains("login"))
                topics.Add("Đăng nhập");
            
            if (lowerMessage.Contains("mật khẩu") || lowerMessage.Contains("password"))
                topics.Add("Quản lý mật khẩu");
            
            if (lowerMessage.Contains("phụ huynh") || lowerMessage.Contains("parent"))
                topics.Add("Quản lý phụ huynh");
            
            if (lowerMessage.Contains("giáo viên") || lowerMessage.Contains("teacher"))
                topics.Add("Quản lý giáo viên");
            
            if (lowerMessage.Contains("học sinh") || lowerMessage.Contains("student"))
                topics.Add("Quản lý học sinh");
            
            if (lowerMessage.Contains("lớp học") || lowerMessage.Contains("class"))
                topics.Add("Quản lý lớp học");
            
            if (lowerMessage.Contains("lịch học") || lowerMessage.Contains("schedule"))
                topics.Add("Quản lý lịch học");
            
            if (lowerMessage.Contains("chương trình") || lowerMessage.Contains("syllabus"))
                topics.Add("Chương trình học");
            
            if (lowerMessage.Contains("thanh toán") || lowerMessage.Contains("payment"))
                topics.Add("Thanh toán học phí");

            return topics;
        }

        private string? GenerateSuggestedActions(string message, string? userRole)
        {
            var lowerMessage = message.ToLower();
            var actions = new List<string>();

            // Phân tích tính cách và sở thích
            if (lowerMessage.Contains("năng động") || lowerMessage.Contains("thể thao") || lowerMessage.Contains("hoạt động"))
            {
                actions.Add("Xem mục 'Chương trình học' để tìm lớp có hoạt động thể thao");
                actions.Add("Tham khảo mục 'Hoạt động ngoại khóa'");
                actions.Add("Liên hệ giáo viên thể dục để tư vấn");
            }

            if (lowerMessage.Contains("học kém") || lowerMessage.Contains("cải thiện") || lowerMessage.Contains("hỗ trợ học"))
            {
                actions.Add("Xem mục 'Hoạt động học tập' để tìm bài tập bổ sung");
                actions.Add("Liên hệ giáo viên bộ môn để được hướng dẫn");
                actions.Add("Trao đổi với giáo viên chủ nhiệm về tình hình học tập");
            }

            if (lowerMessage.Contains("lớp nào") || lowerMessage.Contains("phù hợp") || lowerMessage.Contains("tính cách"))
            {
                actions.Add("Xem mục 'Danh sách lớp học' để tìm lớp phù hợp");
                actions.Add("Tham khảo mục 'Chương trình học' để hiểu nội dung từng lớp");
                actions.Add("Trao đổi với giáo viên chủ nhiệm để được tư vấn");
            }

            // Các chức năng cơ bản
            if (lowerMessage.Contains("đăng ký"))
            {
                if (userRole == "HR")
                    actions.Add("Liên hệ bộ phận nhân sự để tạo tài khoản giáo viên");
                else
                    actions.Add("Truy cập trang web PES và nhấn 'Đăng ký phụ huynh'");
            }

            if (lowerMessage.Contains("đăng nhập"))
                actions.Add("Truy cập trang web PES và nhấn 'Đăng nhập'");

            if (lowerMessage.Contains("mật khẩu"))
            {
                actions.Add("Nhấn 'Quên mật khẩu' trên trang đăng nhập");
                actions.Add("Kiểm tra email để nhận link reset");
            }

            if (lowerMessage.Contains("lớp học") && userRole == "TEACHER")
                actions.Add("Đăng nhập và vào mục 'Lớp học của tôi'");

            if (lowerMessage.Contains("lịch học") && userRole == "TEACHER")
                actions.Add("Đăng nhập và vào mục 'Lịch dạy'");

            if (lowerMessage.Contains("thanh toán"))
                actions.Add("Đăng nhập tài khoản phụ huynh và vào mục 'Thanh toán'");

            // Gợi ý chung cho mọi tình huống
            if (actions.Count == 0)
            {
                actions.Add("Liên hệ giáo viên chủ nhiệm để được tư vấn chi tiết");
                actions.Add("Tham khảo mục 'Hỗ trợ' để tìm thông tin liên quan");
            }

            return actions.Count > 0 ? string.Join("; ", actions) : null;
        }
    }

    // Helper classes for Gemini API response
    public class GeminiResponse
    {
        public Candidate[] candidates { get; set; } = Array.Empty<Candidate>();
    }

    public class Candidate
    {
        public Content content { get; set; } = new();
    }

    public class Content
    {
        public Part[] parts { get; set; } = Array.Empty<Part>();
    }

    public class Part
    {
        public string text { get; set; } = string.Empty;
    }
}
