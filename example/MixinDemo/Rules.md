接口层，一般的名称是xxxx.Core
1. 所有的接口中涉及到的类型需要record类型，并且是public类型的
1. 所有的接口中涉及到的类型都需要定义在接口内部，作为接口的内部类，并且是public类型的
1. 如果接口中的字段涉及到一些约束，例如最大值最小值，是否必填等，需要再接口的模型上定义
1. 接口的命名统一使用IxxxService等这样的格式，例如 IUserService
1. 如果中如果需要定义一些增删改查的方法，优先建议使用接口继承 IQueryPageApi<T>,ICreateApi<T, TKey>,IUpdateApi<T, TKey>,IDeleteApi<TKey>, 如果继承了这些接口，那么需要定义[OperationArgument("name", "xxxx")]标签
      

实现层, 一般的名称是xxxx.Impl
1. 使用AutoConstructor增加构造函数
1. 使用 [Service(typeof(xxx))]注入服务
1. 如果接口继承了 IQueryPageApi<T>,ICreateApi<T, TKey>,IUpdateApi<T, TKey>,IDeleteApi<TKey>，可以直接定义 [Mixin(typeof(QueryApi<LabelEntity, LabelInfo>))],[Mixin(typeof(CreateApi<LabelEntity, CreateLabelInfo, int>))],[Mixin(typeof(UpdateApi<LabelEntity, UpdateLabelInfo, int>))],[Mixin(typeof(DeleteApi<LabelEntity, int>))],并且一行定义一个注解
1. 类型的转换需要使用Mapper定义转换 [Mapper(typeof(LabelEntity), typeof(LabelInfo), MapperType = MapperType.Query)],[Mapper(typeof(CreateLabelInfo), typeof(LabelEntity), MapperType = MapperType.Convert)],[Mapper(typeof(UpdateLabelInfo), typeof(LabelEntity), MapperType = MapperType.Update)]

Controller层
1. 使用ExposeApi导出接口   [ExposeApi(typeof(ILabelService), TypeAttributePatterns = new string[] { "*" }, MethodAttributePatterns = new string[] { "*" })]