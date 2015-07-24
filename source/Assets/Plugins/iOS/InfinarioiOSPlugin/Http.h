//
//  Http.h
//  InfinarioSDK
//
//  Created by Igi on 2/4/15.
//  Copyright (c) 2015 Infinario. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface Http : NSObject

- (instancetype)initWithTarget:(NSString *)target;
- (NSDictionary *)post:(NSString *)url withPayload:(NSDictionary *)payload;
- (NSDictionary *)get:(NSString *)url;

@end
