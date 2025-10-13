import Header from "@/components/Header";
import Footer from "@/components/Footer";

export default function Home() {
  return (
    <div className="min-h-screen flex flex-col">
      <Header />
      <main className="flex-1 container mx-auto px-4 py-8">
        <div className="text-center">
          <h1 className="text-4xl font-bold text-gray-900 mb-4">
            Chào mừng đến với EVB Platform
          </h1>
          <p className="text-xl text-gray-600 mb-8">
            Nền tảng giao dịch xe điện và pin xe điện hàng đầu Việt Nam
          </p>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mt-12">
            <div className="bg-white p-6 rounded-lg shadow-md">
              <h3 className="text-xl font-semibold mb-3">Pin xe điện</h3>
              <p className="text-gray-600">Khám phá các loại pin xe điện chất lượng cao</p>
            </div>
            <div className="bg-white p-6 rounded-lg shadow-md">
              <h3 className="text-xl font-semibold mb-3">Xe điện</h3>
              <p className="text-gray-600">Tìm kiếm xe điện phù hợp với nhu cầu</p>
            </div>
            <div className="bg-white p-6 rounded-lg shadow-md">
              <h3 className="text-xl font-semibold mb-3">Dịch vụ</h3>
              <p className="text-gray-600">Hỗ trợ và bảo hành chuyên nghiệp</p>
            </div>
          </div>
        </div>
      </main>
      <Footer />
    </div>
  );
}
